﻿using System;
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

namespace Instructions.Controllers
{
    public class RecordsController : Controller
        {
            private readonly UserManager<User> _userManager;
            private ApplicationDbContext Recordcontext;
            IConfiguration Configuration;
            static int id;
            

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
                Recordcontext.Records.Add(record);
                await Recordcontext.SaveChangesAsync();
                await CreateSteps( StepName,Text, record);
                if (Tags != null)           
                await CreateTags(record, Tags);
                return RedirectToAction("Index");
            }
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
            var files = Request.Form.Files;
            /*CloudBlobContainer container = GetCloudBlobContainer("images");
            var result=container.CreateIfNotExistsAsync().Result;       
            foreach (var file in files)
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(file.Name);
                await blob.UploadFromStreamAsync(file.OpenReadStream());
            }*/
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


    }
    
}
   