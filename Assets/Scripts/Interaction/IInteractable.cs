public interface IInteractable
{
    bool CanInteract(PlayerInventory inventory); //상호작용 가능한지
    void Interact(PlayerInventory inventory); //상호작용을 하는 메서드
}
