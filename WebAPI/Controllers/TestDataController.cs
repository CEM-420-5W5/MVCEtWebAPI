using Models.Data;
using Models.Models;
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
        [HttpPost]
        public ActionResult<TestData> CreateData(CreateTestDataDTO data)
        {
            TestData testData = new TestData() { Name = data.Name };
            _dbContext.TestDatas.Add(testData);
            _dbContext.SaveChanges();
            return testData;
        }
    }
}
