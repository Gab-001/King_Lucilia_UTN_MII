namespace KingLucilia.Core
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(Player.KingLuciliaController player);
    }
}
