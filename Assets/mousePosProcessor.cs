using UnityEngine.Scripting;

namespace UnityEngine.InputSystem.Processors
{
    /// <summary>
    /// Normalizes a <c>Vector2</c> input value.
    /// </summary>
    /// <remarks>
    /// This processor is registered (see <see cref="InputSystem.RegisterProcessor{T}"/>) under the name "normalizeVector2".
    /// </remarks>
    /// <seealso cref="NormalizeVector3Processor"/>
    public class CenterScreenProcessor : InputProcessor<Vector2>
    {
        /// <summary>
        /// Normalize <paramref name="value"/>. Performs the equivalent of <c>value.normalized</c>.
        /// </summary>
        /// <param name="value">Input vector.</param>
        /// <param name="control">Ignored.</param>
        /// <returns>Normalized vector.</returns>
        public override Vector2 Process(Vector2 value, InputControl control)
        {
            Vector2 screen_center = new Vector2(Screen.width/2, Screen.height/2);
            return (value - screen_center) / screen_center;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CenterScreen()";
        }
    }
}

