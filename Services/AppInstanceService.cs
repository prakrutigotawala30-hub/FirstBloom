namespace FirstBloom.Services
{
    public class AppInstanceService
    {
        public string InstanceId { get; } =
            Guid.NewGuid().ToString();
    }
}