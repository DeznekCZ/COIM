using Mafi.Core.Syncers;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Data.DisplayEntity
{
    public class DisplayEntityInspector : BaseInspector<DisplayEntity>
    {
        private Action m_clear;

        public DisplayEntityInspector(UiContext context) : base(context)
        {
            EmbedStatusToTheTop();

            this.Observe(() => Entity.DisplayManager)
                .Do(manager =>
                {
                    m_clear?.Invoke();
                    if (manager is null) return;

                    m_clear = manager.Inspector(this);
                });
        }
    }
}
