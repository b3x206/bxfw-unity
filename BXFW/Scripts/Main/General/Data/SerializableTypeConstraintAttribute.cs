using System;
using System.Linq;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// This attribute can be used to constraint a <see cref="SerializableType"/> field.
    /// <br>Multiple of these attributes can be used to add multiple constraints to it, but the 'Allow-' prefixed parameters does affect all.</br>
    /// </summary>
    public sealed class SerializableTypeConstraintAttribute : PropertyAttribute
    {
        /// <summary>
        /// Type constraint for requiring inheritance from this type.
        /// </summary>
        public readonly Type inheritFromConstraint;

        public bool AllowGenericTypes { get; set; } = true;
        public bool AllowClass { get; set; } = true;
        public bool AllowEnums { get; set; } = true;
        public bool AllowValueTypes { get; set; } = true;
        public bool AllowInterface { get; set; } = true;
        public bool AllowSealedTypes { get; set; } = true;
        public bool AllowAbstractTypes { get; set; } = true;

        public bool IsTypeAllowed(Type t)
        {
            if (inheritFromConstraint != null)
            {
                if (inheritFromConstraint.IsSealed)
                {
                    throw new InvalidOperationException($"[SerializableTypeConstraintAttribute::IsTypeAllowed] Given type ${inheritFromConstraint} is sealed and is used as the 'inheritConstraint'. Given type must be 'non-sealed'.");
                }
                if (!inheritFromConstraint.GetBaseTypes().Contains(t))
                {
                    return false;
                }
            }

            // yandev experience
            if (!AllowClass && t.IsClass)
            {
                return false;
            }
            if (!AllowEnums && t.IsEnum)
            {
                return false;
            }
            if (!AllowValueTypes && t.IsValueType)
            {
                return false;
            }
            if (!AllowInterface && t.IsInterface)
            {
                return false;
            }
            if (!AllowGenericTypes && t.IsGenericType)
            {
                return false;
            }
            if (!AllowSealedTypes && t.IsSealed)
            {
                return false;
            }
            if (!AllowAbstractTypes && t.IsAbstract)
            {
                return false;
            }

            return true;
        }

        public SerializableTypeConstraintAttribute()
        { }

        public SerializableTypeConstraintAttribute(Type inheritFromConstraint)
        {
            this.inheritFromConstraint = inheritFromConstraint;
        }
    }
}
