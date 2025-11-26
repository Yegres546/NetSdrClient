using NetSdrClient.Models;
using NetSdrClient.UI; // Навмисне порушення - Services залежить від UI

namespace NetSdrClient.Services
{
    public class BadService
    {
        private readonly UIComponent _uiComponent; // Порушення!
        
        public BadService()
        {
            _uiComponent = new UIComponent();
        }
        
        public void DoSomething()
        {
            _uiComponent.ShowMessage("This violates architecture rules!");
        }
    }
}
