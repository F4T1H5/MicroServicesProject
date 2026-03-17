using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Uncomment this line to enable authorization for this controller
    public class AuthorsServiceBaseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet] // HttpGet: attribute also called action method, get route: /Groups
                  //[AllowAnonymous] // Can be used to allow authenticated and unauthenticated users (everyone) to execute this action.
                  // Overrides the Authorize defined for the controller.
        public async Task<IActionResult> Get()
        {
            var query = await _mediator.Send(new GroupQueryRequest());

            var list = await query.ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")] // get route: /Groups/5 (name defined in {} must be same as the action's parameter name, id will be 5)
        public async Task<IActionResult> Get(int id)
        {
            var query = await _mediator.Send(new GroupQueryRequest());

            var item = await query.SingleOrDefaultAsync(groupResponse => groupResponse.Id == id);

            if (item is null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost] // post route: /Groups
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Post(GroupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);

                if (response.IsSuccessful)
                {
                    return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
                }

                return BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        [HttpPut] // put route: /Groups
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Put(GroupUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);

                if (response.IsSuccessful)
                {
                    return NoContent();
                }

                return BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")] // delete route: /Groups/5 (name defined in {} must be same as the action's parameter name, id will be 5)
        [Authorize(Roles = "Admin")] // Only authenticated users with role Admin can execute this action.
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new GroupDeleteRequest() { Id = id });

            if (response.IsSuccessful)
            {
                return NoContent();
            }
            return BadRequest(response);
        }
    }
}
