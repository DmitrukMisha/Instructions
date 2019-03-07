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
            private ApplicationDbContext Recordcontext;
            private readonly IConfiguration Configuration;
            static int id;
            static string MainFileName;
            static FileStream MainFile;
            static string RecordIdForUpdate;
            static List<int> StepsIdForUpdate;
            static List<int> StepsIdForDelete;
            static List<int> TagsIdForDelete;

        public RecordsController(ApplicationDbContext context,UserManager<User> userManager, IConfiguration configuration)
            {     
                Recordcontext = context;
                _userManager = userManager;
                 Configuration = configuration;
            }

          
            public IActionResult Index()
            {
           var tags = Recordcontext.Tags.Select(t => t.TagName).ToList().Distinct();
            ViewBag.Tags =new HtmlString(JsonConvert.SerializeObject(tags,Formatting.None)) ;
            ViewBag.Themes= Recordcontext.Themes.ToList();
            id = 0;
            return View();
            }
      
        public IActionResult RecordEdit(string RecordId)
        {
            StepsIdForUpdate = new List<int>();
            StepsIdForDelete = new List<int>();
            TagsIdForDelete = new List<int>();
            if (RecordId != null)
                RecordIdForUpdate = RecordId;
               var tags = Recordcontext.Tags.Select(t => t.TagName).ToList().Distinct();
            ViewBag.Tags = new HtmlString(JsonConvert.SerializeObject(tags, Formatting.None));
            ViewBag.Themes = Recordcontext.Themes.ToList();
            id = 0;
            Record record = Recordcontext.Records.Where(a => a.RecordID == Int32.Parse(RecordIdForUpdate)).FirstOrDefault();
            List<Step> steps = Recordcontext.Steps.Where(a => a.RecordID == record).ToList();
            ViewBag.Steps = steps;
            ViewData["TagsList"] = TagsList(record);
            
            foreach(Step step in steps)
            {
                StepsIdForUpdate.Add(step.StepID);
            }
            return View(record);
        }

        public string TagsList (Record record)
        {
            string tags="";
            List<Tag> TagsList = Recordcontext.Tags.Where(a => a.Record.RecordID == record.RecordID).ToList();
            foreach(Tag tag in TagsList)
            {
               tags+=tag.TagName;
                TagsIdForDelete.Add(tag.TagID);
            }
            return tags;
        }
        [HttpPost]
        public IActionResult NewStep(string stepId)
        {
            id++;
            ViewData["id"] = id;
            if (stepId == null)
            {
                return PartialView();
            }
            else
            {
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

        public async Task CreateSteps(List<string> StepName, List<string> Text, Record record)
        {
            for(int i=0;i<StepName.Count;i++)
            {
                Step step = new Step
                {
                    Text = Text.ElementAt(i),
                    StepName = StepName.ElementAt(i),
                    RecordID = record
                };
                await Recordcontext.Steps.AddAsync(step);
                
            }
            await Recordcontext.SaveChangesAsync();
        }
        public async Task CreateTags(Record record , string Tags)
        {
    
            Tags=Tags.Replace(",", String.Empty);
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
            if (user != null)
            {

                record.USerID = user.Id;
                record.ImageLink = await CreateImageForRecord(record);
                Recordcontext.Records.Add(record);
                await Recordcontext.SaveChangesAsync();
                await CreateSteps(StepName, Text, record);
                if (Tags != null)
                    await CreateTags(record, Tags);
                return Redirect("/home");
            }
            return Redirect("~/Identity/Account/Login");

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
                if(recordfromdb.ImageLink == null)
               recordfromdb.ImageLink = await CreateImageForRecord(record);
               Recordcontext.Records.Update(recordfromdb);
                await Recordcontext.SaveChangesAsync();
                await UpdateSteps(StepName, Text, recordfromdb);
                if (Tags != null)
                    await UpdateTags(recordfromdb, Tags);
                return Redirect("/home");
            }
            return Redirect("~/Identity/Account/Login");

        }

        [HttpPost]
        public  void DelStepFromDB(int StepID)
        {
            StepsIdForDelete.Add(StepID);
        }

        [HttpPost]
        public void DelPhotoFromDB(int ID, bool IsRecord)
        {
            if (IsRecord)
            {DeleteRecordPhoto(ID);}
           // else {DeleteStepPhoto(ID);}
            Recordcontext.SaveChangesAsync();
        }

        public void DeleteRecordPhoto(int ID)
        {
            Record record = Recordcontext.Records.Where(a => a.RecordID == ID).FirstOrDefault();
            if (record != null)
            {
                record.ImageLink = null;
                Recordcontext.Records.Update(record);
            }
        }

        public void DeleteStepPhoto(int ID)
        {
            Step step = Recordcontext.Steps.Where(a => a.StepID == ID).FirstOrDefault();
            if (step != null)
            {
               // step.ImageLink = null;
                Recordcontext.Steps.Update(step);
            }
        }

        public async Task UpdateSteps(List<string> StepName, List<string> Text, Record record)
        {
            for(int i=0; i<StepsIdForUpdate.Count;i++)
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

            }
            await Recordcontext.SaveChangesAsync();
        }
        public async Task UpdateTags(Record record, string Tags)
        {
            DeleteTagsFromDB();
            Tags = Tags.Replace(",", String.Empty);
            List<string> TagsList = Tags.Split("#").ToList();
            foreach (string Tag in TagsList)
            {
                if (Tag != "")
                {
                    Tag tag = new Tag
                    {
                        Record = record,
                        TagName = "#" + Tag.Replace(" ", String.Empty)
                    };
                     Recordcontext.Tags.Add(tag);
                }

            }
            await Recordcontext.SaveChangesAsync();

        }
        
        private async void DeleteTagsFromDB()
        {
            foreach (int ID in TagsIdForDelete)
            {
                Tag tag = Recordcontext.Tags.Where(a => a.TagID == ID).FirstOrDefault();
                Recordcontext.Tags.Remove(tag);
            }
            await Recordcontext.SaveChangesAsync();
        }

        private async Task<string> CreateImageForRecord(Record record)
        {
            string link=await UploadFile(MainFile,MainFileName);
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
            /*CloudBlobContainer container = GetCloudBlobContainer("images");
            var result=container.CreateIfNotExistsAsync().Result;       
            foreach (var file in files)
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(file.Name);
                await blob.UploadFromStreamAsync(file.OpenReadStream());
            }*/
            files.ToString();
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

        }

        public async Task<string> UploadFile(FileStream file, string fileName)
        {
            using (FileStream fileStream = new FileStream(file.Name, FileMode.Open))
            {
                CloudBlobContainer container = GetCloudBlobContainer("images");
                var result = container.CreateIfNotExistsAsync().Result;
                CloudBlockBlob blob = container.GetBlockBlobReference(DateTime.Now.ToString().Replace(" ", String.Empty) + fileName);
                await blob.UploadFromStreamAsync(fileStream);
                return blob.Uri.ToString();
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
   