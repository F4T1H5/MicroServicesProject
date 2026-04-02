#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using CORE.APP.Models;
using Books.App.Features.Authors;

//Generated from Custom Microservices Template.
namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ILogger<AuthorsController> _logger;
        private readonly IMediator _mediator;

        // Constructor: injects logger to log the errors to Kestrel Console or Output Window and mediator
        public AuthorsController(ILogger<AuthorsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/Authors
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new AuthorQueryRequest());
                // Convert the query response to a list
                var list = await response.ToListAsync();
                // If there are items, return them with 200 OK
                if (list.Any())
                    return Ok(list);
                // If no items found, return 204 No Content
                return NoContent();
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("AuthorsGet Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during AuthorsGet.")); 
            }
        }

        // GET: api/Authors/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new AuthorQueryRequest());
                // Find the item with the given id
                var item = await response.SingleOrDefaultAsync(r => r.Id == id);
                // If item found, return it with 200 OK
                if (item is not null)
                    return Ok(item);
                // If item not found, return 204 No Content
                return NoContent();
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("AuthorsGetById Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during AuthorsGetById.")); 
            }
        }

		// POST: api/Authors
        [HttpPost]
        public async Task<IActionResult> Post(AuthorCreateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the create request
                    var response = await _mediator.Send(request);
                    // If creation is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
                        return Ok(response);
                    }
                    // If creation failed, add error command response message to model state
                    ModelState.AddModelError("AuthorsPost", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("AuthorsPost Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during AuthorsPost.")); 
            }
        }

        // PUT: api/Authors
        [HttpPut]
        public async Task<IActionResult> Put(AuthorUpdateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the update request
                    var response = await _mediator.Send(request);
                    // If update is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return NoContent();
                        return Ok(response);
                    }
                    // If update failed, add error command response message to model state
                    ModelState.AddModelError("AuthorsPut", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("AuthorsPut Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during AuthorsPut.")); 
            }
        }

        // DELETE: api/Authors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Send the delete request
                var response = await _mediator.Send(new AuthorDeleteRequest() { Id = id });
                // If delete is successful, return 200 OK with success command response
                if (response.IsSuccessful)
                {
                    //return NoContent();
                    return Ok(response);
                }
                // If delete failed, add error command response message to model state
                ModelState.AddModelError("AuthorsDelete", response.Message);
                // Return 400 Bad Request with the error command response message
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("AuthorsDelete Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during AuthorsDelete.")); 
            }
        }
	}
}
