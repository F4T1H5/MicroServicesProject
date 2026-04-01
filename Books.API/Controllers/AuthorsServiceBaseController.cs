using Books.App.Features.Authors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AuthorsServiceBaseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorsServiceBaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = await _mediator.Send(new AuthorQueryServiceBaseRequest());

            var list = await query.ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = await _mediator.Send(new AuthorQueryServiceBaseRequest());

            var item = await query.SingleOrDefaultAsync(authorResponse => authorResponse.Id == id);

            if (item is null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post(AuthorCreateServiceBaseRequest request)
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

        [HttpPut]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(AuthorUpdateServiceBaseRequest request)
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

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new AuthorDeleteServiceBaseRequest() { Id = id });

            if (response.IsSuccessful)
            {
                return NoContent();
            }

            return BadRequest(response);
        }
    }
}
