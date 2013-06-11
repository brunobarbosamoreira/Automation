namespace Automation.Modbus.WorkItem
{
    interface IWorkItem
    {
        Address Address { get;  }
        byte[] Buffer { get; set; }
        void UpdateTags();
    }    
}
