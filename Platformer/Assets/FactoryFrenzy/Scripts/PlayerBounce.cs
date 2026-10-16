using StarterAssets;
using Unity.Netcode;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerBounce : NetworkBehaviour
    {
        [UnityEngine.SerializeField] private ThirdPersonController thirdPersonController;

        private static readonly System.Reflection.FieldInfo VerticalVelocityField =
            typeof(ThirdPersonController).GetField("_verticalVelocity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public void Bounce(float velocity)
        {
            if (!IsOwner) return;
            VerticalVelocityField.SetValue(thirdPersonController, velocity);
        }
    }
}