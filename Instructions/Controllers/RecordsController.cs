using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Instructions.Models;
using Instructions.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
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
        static User CreatingUser;
        static string RecordIdForUpdate;
        static List<int> StepsIdForUpdate;
        static List<int> StepsIdForDelete;
        static List<int> TagsIdForUpdate;

        public RecordsController(ApplicationDbContext context,UserManager<User> userManager, IConfiguration configuration)
            {     
                Recordcontext = context;
                _userManager = userManager;
                 Configuration = configuration;
            }



        public IActionResult Index(string userID)
        {
            if (User.Identity.Name == null)
                return Redirect("~/Identity/Account/Login");
            CreatingUser = _userManager.FindByIdAsync(userID).Result;
            if (CreatingUser == null)
                CreatingUser = _userManager.GetUserAsync(User).Result;
            if (!CreatingUser.EmailConfirmed&&!_userManager.GetUserAsync(User).Result.RoleISAdmin)
                return Redirect("~/Home");
            var tags = Recordcontext.Tags.Select(t => t.TagName).ToList().Distinct();
            ViewBag.Tags =new HtmlString(JsonConvert.SerializeObject(tags,Formatting.None)) ;
            ViewBag.Themes= Recordcontext.Themes.ToList();
            activeSteps = new List<int>();
            filePaths = new List<FilePath>();
            id = 0;
            MainFile = null;
            return View();
            }
      
        public IActionResult RecordEdit(string RecordId)
        {
            StepsIdForUpdate = new List<int>();
            StepsIdForDelete = new List<int>();
            TagsIdForUpdate = new List<int>();
            activeSteps = new List<int>();
            filePaths = new List<FilePath>();
            if (RecordId != null)
                RecordIdForUpdate = RecordId;
               var tags = Recordcontext.Tags.Select(t => t.TagName).ToList().Distinct();
            ViewBag.Tags = new HtmlString(JsonConvert.SerializeObject(tags, Formatting.None));
            ViewBag.Themes = Recordcontext.Themes.ToList();
            MainFile = null;
            Record record = Recordcontext.Records.Where(a => a.RecordID == Int32.Parse(RecordIdForUpdate)).FirstOrDefault();
            List<Step> steps = Recordcontext.Steps.Where(a => a.RecordID == record).ToList();
            ViewBag.Steps = steps;
            ViewData["TagsList"] = TagsList(record);
            foreach(Step step in steps)
            {
                StepsIdForUpdate.Add(step.StepID);
            }
            id = StepsIdForUpdate.Max();
            return View(record);
        }

        public string TagsList (Record record)
        {
            string tags="";
            List<Tag> TagsList = Recordcontext.Tags.Where(a => a.Record.RecordID == record.RecordID).ToList();
            foreach(Tag tag in TagsList)
            {
               tags+=tag.TagName;
                TagsIdForUpdate.Add(tag.TagID);
            }
            return tags;
        }
        [HttpPost]
        public IActionResult NewStep(string stepId)
        {
            id++;
            
            if (stepId == null)
            {
                activeSteps.Add(id);
                ViewData["id"] = id;
                return PartialView();
            }
            else
            { 
                ViewBag.Images = Recordcontext.Images.Where(a => a.StepID.StepID == Int32.Parse(stepId)).ToList();
                activeSteps.Add(Int32.Parse(stepId));
                ViewData["id"] = stepId;
                Step step = Recordcontext.Steps.Where(a => a.StepID == int.Parse(stepId)).FirstOrDefault();
                return PartialView(step);
            }
        }


        [HttpPost]
        public IActionResult Photo(string Id, string Link, bool IsRecord)
        {
            ViewData["PhotoID"] = Id;
            ViewData["Link"] = Link;
            ViewData["IsRecord"] = IsRecord;
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
            int index = -1;
            int maxId = filePaths.Max(a => a.id);
            if (!activeSteps.Contains(idnew))
                index = SearchIndex(ref idnew, maxId);
            else { index = filePaths.FindIndex(x => x.id == idnew); }
            if (idnew > maxId) return -1;
            if (!(index == -1))
                await UploadStepImage(index, idnew, step);

            idnew++;
            return idnew;

        }

        public int SearchIndex(ref int idnew, int maxId)

        {
            int index = -1;            
            do
            {
                if (index == -1)
                {
                    idnew++;
                    int id = idnew;
                    index = filePaths.FindIndex(x => x.id == id);
                }
            }
            while (index == -1 && idnew <= maxId && !activeSteps.Contains(idnew));
            return index;
        }
        public async Task  UploadStepImage(int index, int idnew, Step step)
        {
            while (index < filePaths.Count && filePaths.ElementAt(index).id == idnew)
            {
                string link = await UploadFile(filePaths.ElementAt(index).path, filePaths.ElementAt(index).filename);
                if (link != null)
                {
                    Image image = new Image { StepID = step, Link = link };
                    Recordcontext.Images.Add(image);
                    await Recordcontext.SaveChangesAsync();
                }
                index++;
            }
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
           
                record.USerID = CreatingUser.Id;
                if (MainFile!=null)
                record.ImageLink=await CreateImageForRecord(record);                
                Recordcontext.Records.Add(record);              
                await Recordcontext.SaveChangesAsync();
                await CreateSteps( StepName,Text, record);
                if (Tags != null)           
                await CreateTags(record, Tags);
                return Redirect("/home");
            }

        [HttpPost]
        public async Task<IActionResult> Update(Record record, List<string> StepName, List<string> Text, string Tags)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Record recordfromdb = Recordcontext.Records.Where(a => a.RecordID == Int32.Parse(RecordIdForUpdate)).FirstOrDefault();
                recordfromdb.Description = record.Description;
                recordfromdb.Name = record.Name;
                recordfromdb.ThemeName = record.ThemeName;
                if(recordfromdb.ImageLink == null&&MainFile!=null)
                recordfromdb.ImageLink = await CreateImageForRecord(record);
                Recordcontext.Records.Update(recordfromdb);
                await Recordcontext.SaveChangesAsync();
                await UpdateSteps(StepName, Text, recordfromdb);
                await UpdateTags(recordfromdb, Tags);
                return Redirect("/home");
            }
            return Redirect("~/Identity/Account/Login");

        }

        

        [HttpPost]
        public  async Task DelStepFromDB(int StepID)
        {
           await DeleteStepPhotos(StepID);
            StepsIdForDelete.Add(StepID);
        }
        public async Task DeleteStepPhotos(int stepId)
        {
             var images= Recordcontext.Images.Where(a => a.StepID.StepID == stepId).ToList();
            foreach (var image in images)
                Recordcontext.Images.Remove(image);
            await Recordcontext.SaveChangesAsync();

        }

        public async Task UpdateSteps(List<string> StepName, List<string> Text, Record record)
        {
            List<FilePath> filePathsSorted = new List<FilePath>();
            if (filePaths != null)
                filePathsSorted = (from filepath in filePaths orderby filepath.id select filepath).ToList();
            int index = 1;
            for (int i=0; i<StepsIdForUpdate.Count;i++)
            {
               
                Step step = Recordcontext.Steps.Where(a => a.StepID == StepsIdForUpdate.ElementAt(i)).FirstOrDefault();
                if (StepsIdForDelete.Contains(StepsIdForUpdate.ElementAt(i)))
                {
                    Recordcontext.Steps.Remove(step);
                }
                else
                {
                    step.StepName = StepName.ElementAt(i);
                    step.Text = Text.ElementAt(i);
                    Recordcontext.Steps.Update(step);
                    if (index != -1 && filePathsSorted.Count != 0)
                        index = await CreateImagesForStep(filePathsSorted, step, index);
                }
            }
            int CountDefaultSteps = StepsIdForUpdate.Count - StepsIdForDelete.Count;
            for (int i = CountDefaultSteps; i < StepName.Count; i++)
            {
                Step step = new Step
                {
                    Text = Text.ElementAt(i),
                    StepName = StepName.ElementAt(i),
                    RecordID = record
                };
                Recordcontext.Steps.Add(step);
                if (index != -1 && filePathsSorted.Count != 0)
                    index = await CreateImagesForStep(filePathsSorted, step, index);
            }
            
            await Recordcontext.SaveChangesAsync();
            
        }
        public async Task UpdateTags(Record record, string Tags)
        {
            if (Tags != null)
            {
                Tags = Tags.Replace(",", String.Empty);
                List<string> TagsList = Tags.Split("#").ToList();
                TagsList.Remove("");
                int u = TagsList.Count() - TagsIdForUpdate.Count();
                for (int i = 0; i < TagsList.Count(); i++)
                {
                    if (i < TagsIdForUpdate.Count())
                    {
                        Tag tagfromdb = Recordcontext.Tags.Where(a => a.TagID == TagsIdForUpdate.ElementAt(i)).FirstOrDefault();
                        if ("#" + TagsList.ElementAt(i).Replace(" ", String.Empty) != tagfromdb.TagName)
                        {
                            tagfromdb.TagName = "#" + TagsList.ElementAt(i).Replace(" ", String.Empty);
                            Recordcontext.Tags.Update(tagfromdb);
                        }
                    }
                    else
                    {
                        Tag tag = new Tag
                        {
                            Record = record,
                            TagName = "#" + TagsList.ElementAt(i).Replace(" ", String.Empty)
                        };
                        Recordcontext.Tags.Add(tag);
                    }
                }
                if (u < 0)
                {
                    for (int i = TagsList.Count(); i < TagsIdForUpdate.Count(); i++)
                    {
                        Tag TagForDelete = Recordcontext.Tags.Where(a => a.TagID == TagsIdForUpdate.ElementAt(i)).FirstOrDefault();
                        Recordcontext.Tags.Remove(TagForDelete);
                    }
                }
                await Recordcontext.SaveChangesAsync();
            }
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
        public async Task<ActionResult> CreateTheme(string theme_name)
        {
            if (User != null)
            {
                Theme theme = new Theme
                {
                    Themes = theme_name
                };
                Recordcontext.Themes.Add(theme);


                await Recordcontext.SaveChangesAsync();
            }
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
                    CloudBlockBlob blob = container.GetBlockBlobReference(DateTime.Now.ToString().Replace(" ", String.Empty).GetHashCode()+new Random().Next().GetHashCode()+ fileName);
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
   