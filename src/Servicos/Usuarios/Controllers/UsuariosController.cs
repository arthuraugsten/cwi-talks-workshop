using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NucleoCompartilhado.EventBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usuarios.Dados.Contextos;
using Usuarios.Dominio.Entities;
using Usuarios.Dtos;
using Usuarios.Eventos;

namespace Usuarios.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UsuariosController : ControllerBase
    {
        private readonly UsuáriosContexto _contexto;
        private readonly IEventBus _eventBus;

        public UsuariosController(UsuáriosContexto contexto, IEventBus eventBus)
        {
            _contexto = contexto;
            _eventBus = eventBus;
        }

        [HttpPost("criar")]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Usuário>> PostAsync([FromBody] CriarUsuárioDto usuário)
        {
            if (await _contexto.Usuários.AnyAsync(u => u.Nome == usuário.Nome))
                return BadRequest($"Usuário com nome {usuário.Nome} já existe");

            var novoUsuário = new Usuário(usuário.Nome, usuário.Senha);
            await _contexto.Usuários.AddAsync(novoUsuário);
            await _contexto.SaveChangesAsync();

            _eventBus.Publicar(new UsuarioCriadoEventoIntegracao(novoUsuário.Id, novoUsuário.Nome));

            return Ok(novoUsuário);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Usuário>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Usuário>>> GetAsync()
            => Ok(await _contexto.Usuários.ToListAsync());

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Usuário), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Usuário>> GetAsync([FromRoute] Guid id)
        {
            if (await _contexto.Usuários.FindAsync(id) is { } usuário)
                return Ok(usuário);

            return NotFound();
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(Usuário), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Usuário>> PatchAsync([FromRoute] Guid id, [FromBody] AtualizarUsuárioDto usuário)
        {
            var usuárioEdição = await _contexto.Usuários.FindAsync(id);
            if (usuárioEdição is null)
                return NotFound("Usuário inexistente");

            usuárioEdição.Atualizar(usuário.Nome, usuário.Ativo);
            await _contexto.SaveChangesAsync();

            _eventBus.Publicar(new UsuarioEditadoEventoIntegracao(id, usuário.Nome, usuário.Ativo));

            return Ok(usuárioEdição);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var usuárioInativação = await _contexto.Usuários.FindAsync(id);
            if (usuárioInativação is null)
                return NotFound("Usuário inexistente");

            usuárioInativação.Inativar();
            await _contexto.SaveChangesAsync();

            _eventBus.Publicar(new UsuarioEditadoEventoIntegracao(id, usuárioInativação.Nome, usuárioInativação.Ativo));

            return NoContent();
        }
    }
}
