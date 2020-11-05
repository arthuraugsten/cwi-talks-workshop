using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tarefas.Dados.Contextos;
using Tarefas.Dominio.Entities;
using Tarefas.Dtos;

namespace Tarefas.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly TarefasContexto _contexto;

        public UsuariosController(TarefasContexto contexto) => _contexto = contexto;

        [HttpPost("{idUsuário:guid}/criar")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Usuário), StatusCodes.Status200OK)]
        public async Task<ActionResult<Usuário>> PostAsync([FromRoute] Guid idUsuário, [FromBody] CriarTarefaDto tarefa)
        {
            var usuário = await _contexto.Usuários.Include(p => p.Tarefas).SingleOrDefaultAsync(u => u.Id == idUsuário && u.Ativo);
            if (usuário is null)
                return NotFound($"Usuário inexistente e/ou inativo");

            usuário.AdicionarTarefa(tarefa.Descrição);
            await _contexto.SaveChangesAsync();

            return Ok(usuário);
        }

        [HttpGet("{idUsuário:guid}/listar")]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Tarefa>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetAsync([FromRoute] Guid idUsuário)
        {
            var usuário = await _contexto.Usuários.Include(p => p.Tarefas).SingleOrDefaultAsync(u => u.Id == idUsuário);
            if (usuário is null)
                return NotFound();

            return Ok(usuário.Tarefas);
        }

        [HttpDelete("{idUsuário:guid}/deletar/{id:guid}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync([FromRoute] Guid idUsuário, [FromRoute] Guid id)
        {
            var usuário = await _contexto.Usuários.Include(p => p.Tarefas).SingleOrDefaultAsync(u => u.Id == idUsuário);
            if (usuário is null)
                return NotFound($"Usuário inexistente e/ou inativo");

            if (usuário.RemoverTarefa(id))
                await _contexto.SaveChangesAsync();

            return NoContent();
        }
    }
}
