using Microsoft.Practices.Prism.Logging;

namespace Intellidesk.AcadNet.Infrastructure
{
   public class CustomLogger:ILoggerFacade 

{ 

  
protected static readonly ILog log = LogManager.GetLogger(typeof(CustomLogger)); 
  

public void Log(string message, Category category, Priority priority) 

{ 

//log4net.Config.XmlConfigurator.Configure(); 

switch(category) 

{ 

case Category.Debug: 

log.Debug(message); 

break; 

case Category.Warn: 

log.Warn(message); 

break; 

case Category.Exception: 

log.Error(message); 

break; 

case Category.Info: 

log.Info(message); 

break; 

} 

} 
}
