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
        [HttpPost]
        public IActionResult NewStep()
        {
            id++;
            ViewData["id"] = id;
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
                record.USerID = user.Id;
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
       
    }
    
}
   