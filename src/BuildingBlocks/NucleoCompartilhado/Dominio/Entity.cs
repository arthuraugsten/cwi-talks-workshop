using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NucleoCompartilhado.Dominio
{
    public abstract class Entity : IEquatable<Entity>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public override bool Equals(object obj)
            => Equals(obj as Entity);

        public bool Equals(Entity obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return Id.Equals(obj.Id);
        }

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Id);

        public static bool operator ==(Entity a, Entity b)
        {
            var referenciaA = a is null;
            var referenciaB = b is null;

            if (referenciaA && referenciaB)
                return true;
            if (referenciaA || referenciaB)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
            => !(a == b);
    }
}