using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    [Table("Todo")]
    public class NotesController : BaseController
    {

        [HttpGet]
        public async Task<ActionResult<List<NoteModel>>> Get() {
            try
            {
                var result = await DbContext.Notes.ToListAsync();

                return result;
            }   
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NoteModel>> GetSingle(int id)
        {
            try
            {
                var result = await DbContext.Notes.SingleOrDefaultAsync(s => s.ID == id);

                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<int>> DeleteSingle(int id)
        {
            try
            {
                var entityToDelete = await DbContext.Notes.SingleOrDefaultAsync(s => s.ID == id);

                if (entityToDelete == null) return NotFound();

                DbContext.Notes.Remove(entityToDelete);
                await DbContext.SaveChangesAsync();

                return Ok(id);

            }
            catch (DbException)
            {
                return StatusCode(404, $"Note with id {id} does not exist");
            }
            catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting record");
            }
        }

        [HttpPost]
        public async Task<ActionResult<NoteModel>> PostSingle(NoteModel newNote)
        {
            try
            {
   

                await DbContext.Notes.AddAsync(newNote);
                await DbContext.SaveChangesAsync();

                return Ok(newNote);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating record");
            }
        }

    }
}
