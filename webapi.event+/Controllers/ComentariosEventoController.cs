using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.Text;
using webapi.event_.Domains;
using webapi.event_.Repositories;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventoController : ControllerBase
    {
        ComentariosEventoRepository comentarios = new ComentariosEventoRepository();

        private readonly ContentModeratorClient _contentModeratorClient;

        public ComentariosEventoController(ContentModeratorClient contentModeratorClient)
        {
            _contentModeratorClient = contentModeratorClient;
        }

        [HttpPost("ComentarioIA")]

        public async Task<IActionResult> PostIA(ComentariosEvento novoComentario)
        {
            try
            {
                if(string.IsNullOrEmpty(novoComentario.Descricao))
                {
                    return BadRequest("A descricao do comentario nao pode estar vazio ou nulo");
                }

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(novoComentario.Descricao));

                var moderationResult = await _contentModeratorClient.TextModeration
                    .ScreenTextAsync("text/plain", stream, "por", false, false, null, true);

                if(moderationResult.Terms != null)
                {
                    novoComentario.Exibe = false;
                    comentarios.Cadastrar(novoComentario);
                }
                else
                {
                    novoComentario.Exibe = true;

                    comentarios.Cadastrar(novoComentario);
                }

                return StatusCode(201, novoComentario);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                return Ok(comentarios.Listar(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ListarSomenteExibe/{id}")]
        public IActionResult GetShow(Guid id)
        {
            try
            {
                return Ok(comentarios.ListarSomenteExibe(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("BuscarPorIdUsuario")]

        public IActionResult GetByIdUser(Guid idUsuario, Guid idEvento)
        {
            try
            {
                return Ok(comentarios.BuscarPorIdUsuario(idUsuario, idEvento));
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpPost]

        public IActionResult Post(ComentariosEvento novoComentario)
        {
            try
            {
                comentarios.Cadastrar(novoComentario);

                return StatusCode(201, novoComentario);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]

        public IActionResult Delete(Guid id)
        {
            try
            {
                comentarios.Deletar(id);

                return NoContent();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
