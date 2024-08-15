using AdminMVC.Data;
using AdminMVC.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestDataController : ControllerBase
    {
        protected readonly ApplicationDbContext _dbContext;
        
        public TestDataController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TestData>> GetAll()
        {
            return _dbContext.TestDatas.ToList(); 
        }

        [Authorize]
        [HttpGet("{name}")]
        public ActionResult<TestData> CreateData(string name)
        {
            TestData testData = new TestData() { Name = name };
            _dbContext.TestDatas.Add(testData);
            _dbContext.SaveChanges();
            return testData;
        }

        
    }
}
