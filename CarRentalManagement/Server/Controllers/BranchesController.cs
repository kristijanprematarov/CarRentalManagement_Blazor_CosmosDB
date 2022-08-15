using CarRentalManagement.Shared.DocumentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CarRentalManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _documentContainer;

        public BranchesController(IMemoryCache cache, CosmosClient cosmosClient)
        {
            _cache = cache;
            _cosmosClient = cosmosClient;
            _documentContainer = cosmosClient.GetContainer("Branches", "Locations");
        }

        // GET: api/<BranchesController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var branches = new List<Branch>();

            var branchDocuments = _documentContainer.GetItemQueryIterator<Branch>();

            while (branchDocuments.HasMoreResults)
            {
                var branchesFromCosmos = await branchDocuments.ReadNextAsync();
                branches.AddRange(branchesFromCosmos.Resource);
            }

            return Ok(branches);
        }

        // search: api/<BranchesController>
        [HttpGet("search/{country}")]
        public async Task<IActionResult> SearchByCountry(string country)
        {
            var branches = new List<Branch>();

            var requestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(country)
            };

            var branchDocuments = _documentContainer
                .GetItemQueryIterator<Branch>(requestOptions: requestOptions);

            while (branchDocuments.HasMoreResults)
            {
                var branchesFromCosmos = await branchDocuments.ReadNextAsync();
                branches.AddRange(branchesFromCosmos.Resource);
            }

            return Ok(branches);
        }

        // GET api/<BranchesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<BranchesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Branch branch)
        {
            branch.Id = System.Guid.NewGuid();

            await _documentContainer.CreateItemAsync(branch, new PartitionKey(branch.Country));

            return NoContent();
        }

        // PUT api/<BranchesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BranchesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
