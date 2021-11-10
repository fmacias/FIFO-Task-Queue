/**
 * LICENSE
 *
 * This source file is subject to the new BSD license that is bundled
 * with this package in the file LICENSE.txt.
 *
 * @copyright   Copyright (c) 2021. Fernando Macias Ruano.
 * @E-Mail      fmaciasruano@gmail.com > .
 * @license    https://github.com/fmacias/Scheduler/blob/master/Licence.txt
 */
using System.Threading.Tasks;

namespace fmacias
{
    internal interface ITaskShedulerWraper
    {
        /// <summary>
        /// GUI (graphical user interface) application frameworks for the .NET Framework, 
        /// such as Windows Forms and Windows Presentation Foundation (WPF), let you access 
        /// GUI objects only from the thread that creates and manages the UI (the Main or UI thread)
        /// <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception?view=net-5.0"/>
        /// </summary>
        /// <returns>TaskScheduler <see cref="T:System.Threading.Tasks.TaskScheduler" /></returns>
        TaskScheduler FromGUIWorker();
        /// <summary>
        /// Extract the System.Threading.Tasks.TaskScheduler that belongs to the current Task
        /// </summary>
        /// <returns>TaskScheduler <see cref="T:System.Threading.Tasks.TaskScheduler" /></returns>
        TaskScheduler FromCurrentWorker();
        TaskScheduler Sheduler { get; }
    }
}
