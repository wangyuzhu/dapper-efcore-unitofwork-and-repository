﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlutoData.Collections;
using PlutoData.Interface;

namespace apisample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IUnitOfWork<BloggingContext> _unitOfWork;
        private ILogger<ValuesController> _logger;
        private readonly ICustomBlogRepository _customBlogRepository;

        public ValuesController(IUnitOfWork<BloggingContext> unitOfWork, ILogger<ValuesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _customBlogRepository = unitOfWork.GetRepository<ICustomBlogRepository>();
        }

        // GET api/values
        [HttpGet]
        public async Task<IList<Blog>> Get()
        {
            using (var tran=await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    _customBlogRepository.Insert(new Blog
                    {
                        Url = "Normal_"+new Random().Next(100,999),
                        Title = "498733333353953",
                    });
                    _unitOfWork.SaveChanges(); // 主表
                    var blog10086 = new Blog
                    {
                        Url = "1001_" + new Random().Next(100, 999),
                        Title = "4444444444"
                    };
                    _customBlogRepository.RouteKey = "1001";
                    _customBlogRepository.Insert(blog10086);
                    _unitOfWork.SaveChanges(); // 分表
                    await tran.CommitAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e,e.Message);
                   await tran.RollbackAsync();
                }
            }

            return await _customBlogRepository.GetAllAsync(include: source => source.Include(blog => blog.Posts).ThenInclude(post => post.Comments));
        }



        // GET api/values/Page/5/10
        [HttpGet("Page/{pageIndex}/{pageSize}")]
        public async Task<IPagedList<Blog>> Get(int pageIndex, int pageSize)
        {
            var items = _customBlogRepository.GetPagedList(b => new { Name = b.Title, Link = b.Url });

            return await _customBlogRepository.GetPagedListAsync(pageIndex: pageIndex, pageSize: pageSize);
        }


        // GET api/values/Search/a1
        [HttpGet("Search/{term}")]
        public async Task<IPagedList<Blog>> Get(string term)
        {
            _logger.LogInformation("demo about first or default with include");

            var item = _customBlogRepository.GetFirstOrDefault(predicate: x => x.Title.Contains(term), include: source => source.Include(blog => blog.Posts).ThenInclude(post => post.Comments));

            _logger.LogInformation("demo about first or default without include");

            item = _customBlogRepository.GetFirstOrDefault(predicate: x => x.Title.Contains(term), orderBy: source => source.OrderByDescending(b => b.Id));

            _logger.LogInformation("demo about first or default with projection");

            var projection = _customBlogRepository.GetFirstOrDefault(b => new { Name = b.Title, Link = b.Url }, predicate: x => x.Title.Contains(term));

            return await _customBlogRepository.GetPagedListAsync(predicate: x => x.Title.Contains(term));
        }


        // GET api/values/4
        [HttpGet("{id}")]
        public async Task<Blog> Get(int id)
        {
            return await _customBlogRepository.FindAsync(new object[] { id });
        }

        [HttpPost("/v1/post")]
        public async Task<IActionResult> PostV1()
        {
            var blog2 = new Blog
            {
                Url = "Normal_"+new Random().Next(100,999),
                Title = "12312"
            };
            _customBlogRepository.Insert(blog2);
            await _unitOfWork.SaveChangesAsync();
            return Ok("123");
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]Blog value)
        {
            try
            {
                var strategy = _unitOfWork.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    Guid transactionId;
                    using (var transaction = await _unitOfWork.BeginTransactionAsync())
                    {
                        var blog2 = new Blog
                        {
                            Id = (int)DateTime.Now.Ticks,
                            Url = "1212",
                            Title = "12312"
                        };
                        _customBlogRepository.Insert(blog2);
                        await _unitOfWork.SaveChangesAsync();
                        Thread.Sleep(1000);

                        _customBlogRepository.Insert(value);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync(transaction);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await _unitOfWork.SaveChangesAsync();
        }



        /*
         * 

            //_unitOfWork.ChangeDatabase("PlutoDataDemo_2020");


            //userRepo.ChangeTable("Blogs_10086");
            var blog10086 = new Blog
            {
                Id = (int)DateTime.Now.Ticks % 100,
                Url = "1212",
                Title = "12312"
            };
            _customBlogRepository.Insert(blog10086);

            _unitOfWork.SaveChanges();


            //userRepo.ChangeTable("Blogs_10087");
            var blog10087 = new Blog
            {
                Id = (int)DateTime.Now.Ticks % 10,
                Url = "1212",
                Title = "12312"
            };
            _customBlogRepository.Insert(blog10087);

            await _unitOfWork.SaveChangesAsync();
         */


    }
}