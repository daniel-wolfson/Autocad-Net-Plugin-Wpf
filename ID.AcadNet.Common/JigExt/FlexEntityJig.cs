using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;

namespace Intellidesk.AcadNet.Common.Jig
{
    internal class FlexEntityJig : EntityJig
    {
        protected JigPromptOptions Options { get; }

        protected Func<Entity, PromptResult, bool> UpdateAction { get; }

        protected PromptResult JigResult { get; set; }

        protected string JigResultValue { get; set; }

        public FlexEntityJig(
            JigPromptOptions options,
            Entity entity,
            Func<Entity, PromptResult, bool> updateAction)
            : base(entity)
        {
            Options = options;
            UpdateAction = updateAction;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            string jigResultValue = null;

            if (Options is JigPromptPointOptions pointOptions)
            {
                var result = prompts.AcquirePoint(pointOptions);
                JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (Options is JigPromptDistanceOptions distanceOptions)
            {
                var result = prompts.AcquireDistance(distanceOptions);
                JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (Options is JigPromptAngleOptions angleOptions)
            {
                var result = prompts.AcquireAngle(angleOptions);
                JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (Options is JigPromptStringOptions stringOptions)
            {
                var result = prompts.AcquireString(stringOptions);
                JigResult = result;
                jigResultValue = result.StringResult;
            }

            if (jigResultValue == null)
            {
                return SamplerStatus.Cancel;
            }
            else if (jigResultValue != JigResultValue)
            {
                JigResultValue = jigResultValue;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            return UpdateAction(Entity, JigResult);
        }
    }
}
