using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Instructions.Models;
using Instructions.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace Instructions.Controllers
{
    public class RecordsController : Controller
        {
            private readonly UserManager<User> _userManager;
            static List<FilePath> filePaths;
            private ApplicationDbContext Recordcontext;
            private readonly IConfiguration Configuration;
            static int id;
            static string MainFileName;
            static FileStream MainFile;
             static List<int> activeSteps;
            
           
            

        public RecordsController(ApplicationDbContext context,UserManager<User> userManager, IConfiguration configuration)
            {
                
                Recordcontext = context;
                _userManager = userManager;
                 Configuration = configuration;

        }

          
            public IActionResult Index()
            {
          if (User.Identity.Name == null)
                return Redirect("~/Identity/Account/Login");
           var tags = Recordcontext.Tags.Select(t => t.TagName).ToList().Distinct();
            ViewBag.Tags =new HtmlString(JsonConvert.SerializeObject(tags,Formatting.None)) ;
            ViewBag.Themes= Recordcontext.Themes.ToList();
            activeSteps = new List<int>();
            filePaths = new List<FilePath>();
            id = 0;
            MainFile = null;
            return View();
            }

        [HttpPost]
        public IActionResult NewStep()
        {
            id++;
            activeSteps.Add(id);
            ViewData["id"] = id;
           return PartialView();
        }
        
        [HttpPost]
        public void RemoveStepIdFromList(int id)
        {
            activeSteps.Remove(id);
        }

        public async Task CreateSteps(List<string> StepName, List<string> Text, Record record)
        {
            List<FilePath> filePathsSorted=new List<FilePath>();
            if (filePaths!=null)
            filePathsSorted = (from filepath in filePaths orderby filepath.id select filepath).ToList();
            int index=1;
            for(int i=0;i<StepName.Count;i++)
            {
                Step step = new Step
                {
                    Text = Text.ElementAt(i),
                    StepName = StepName.ElementAt(i),
                    RecordID = record
                };
                await Recordcontext.Steps.AddAsync(step);
                await Recordcontext.SaveChangesAsync();
                if (index!=-1&&filePathsSorted.Count!=0)
                index =await CreateImagesForStep(filePathsSorted, step, index);
            }
           
        }

        public async Task<int> CreateImagesForStep(List<FilePath> filePaths, Step step, int id)
        {
            int idnew = id;
            int index;
            int maxId = filePaths.Max(a => a.id);
            if (!activeSteps.Contains(idnew))
                do
                {
                    index = filePaths.FindIndex(x => x.id == idnew);
                    if (index == -1) idnew++;
                }
                while (index == -1 && idnew <= maxId&&!activeSteps.Contains(idnew));
            else { index = filePaths.FindIndex(x => x.id == idnew); }
            if (idnew > maxId) return -1;
            if (!(index==-1)) 
            while (index<filePaths.Count&&filePaths.ElementAt(index).id == idnew) 
            {
                string link=await UploadFile(filePaths.ElementAt(index).path, filePaths.ElementAt(index).filename);
                    if (link != null)
                    {
                        Image image = new Image { StepID = step, Link = link };
                        Recordcontext.Images.Add(image);                   
                         await Recordcontext.SaveChangesAsync();
                    }
                    index++;
            }
            idnew++;
            return idnew;
            
        }

        public async Task CreateTags(Record record , string Tags)
        {
    
            Tags=Tags.Replace(",", String.Empty);
            Tags = Tags.Replace(" ", String.Empty);
            List<string> TagsList = Tags.Split("#").ToList();
            foreach(string Tag in TagsList)
            {
                if (Tag!="")
                {
                    Tag tag = new Tag
                    {
                        Record = record,
                        TagName = "#" + Tag.Replace(" ", String.Empty)
                    };
                    await Recordcontext.Tags.AddAsync(tag);
                }
               
            }
            await Recordcontext.SaveChangesAsync();
                
        }

        [HttpPost]
           public async Task<IActionResult> Create(Record record, List<string> StepName, List<string> Text, string Tags)
            {
                User user =await _userManager.GetUserAsync(User);         
                record.USerID = user.Id;
                if (MainFile!=null)
                record.ImageLink=await CreateImageForRecord(record);                
                Recordcontext.Records.Add(record);              
                await Recordcontext.SaveChangesAsync();
                await CreateSteps( StepName,Text, record);
                if (Tags != null)           
                await CreateTags(record, Tags);
                return Redirect("/home");
            }

        private async Task<string> CreateImageForRecord(Record record)
        {
            string link=await UploadFile(MainFile.Name,MainFileName);
            return link;        }
        
        private CloudBlobContainer GetCloudBlobContainer(string RecordID)
        {
            CloudStorageAccount storageAccount =
            CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorageConnectionString-1"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(RecordID);
            return container;
        }
        [HttpPost]
        public async Task UploadFiles()
        {
            var MainPic = Request.Form.Files;
            MainFileName = MainPic.ElementAt(0).FileName;
            MainFile = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate);
            await MainPic.ElementAt(0).CopyToAsync(MainFile);
            MainFile.Close();
        }
        [HttpPost]
        public async Task UploadFilesFromStep()
        {
            var files = Request.Form.Files;
            string path = Path.GetTempFileName();  
            using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                await files.ElementAt(0).CopyToAsync(fs);
                fs.Close();
                }
            filePaths.Last().path = path;
            
        }
        [HttpPost]
        public void RemoveFileFromStep(string id,string filename)
        {
            int index = filePaths.FindIndex(a => a.id == int.Parse(id) && a.filename == filename);
            filePaths.RemoveAt(index);
        }


        [HttpPost]
        public async Task<ActionResult> Delete(string[] selected)
        {
            if (selected != null)
            {
                foreach (var id in selected)
                {
                    var record = await Recordcontext.Records.FindAsync(Int32.Parse(id));
                    
                    
                    if (record != null)
                    {
                        List<Step> steps = Recordcontext.Steps.Where(a => a.RecordID == record).ToList();
                        foreach(var step in steps)
                        {
                            Recordcontext.Steps.Remove(step);
                        }
                        Recordcontext.Records.Remove(record);
                        await Recordcontext.SaveChangesAsync();
                    }
                }
            }
            return Redirect("~/Identity/Account/Manage/PersonalInstructions");
        }


        [HttpPost]
        public async Task<ActionResult> CreateTheme(string theme_name)
        {
            Theme theme = new Theme
            {
                Themes = theme_name        
            };
               Recordcontext.Themes.Add(theme);

            
            await Recordcontext.SaveChangesAsync();
            return Redirect("~/Home/AddTheme");
        }


        public void GetData(string id,string filename)
        {
            FilePath filePath = new FilePath
            {
                id = int.Parse(id),
                filename = filename
            };
            filePaths.Add(filePath);
        }

        public async Task<string> UploadFile(string file, string fileName)
        {
            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                {
                    CloudBlobContainer container = GetCloudBlobContainer("images");
                    var result = container.CreateIfNotExistsAsync().Result;
                    CloudBlockBlob blob = container.GetBlockBlobReference(DateTime.Now.ToString().Replace(" ", String.Empty) + fileName);
                    await blob.UploadFromStreamAsync(fileStream);
                    return blob.Uri.ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

       

        [HttpPost]
        public async Task<ActionResult> DeleteTheme(string[] selected)
        {

            if (selected != null)
            {
                foreach (var id in selected)
                {
                    Theme theme = await Recordcontext.Themes.FindAsync(Int32.Parse(id));
                    if (theme != null)
                    {
                        Recordcontext.Themes.Remove(theme);
                    }
                }
            }
            await Recordcontext.SaveChangesAsync();
            return Redirect("~/Home/AddTheme");
        }

    }
    
}
   