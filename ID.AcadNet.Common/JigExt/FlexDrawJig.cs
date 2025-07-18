using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using System;

namespace Intellidesk.AcadNet.Common.Jig
{
    internal class FlexDrawJig : DrawJig
    {
        protected JigPromptOptions Options { get; }

        protected Func<PromptResult, Drawable> UpdateAction { get; }

        protected PromptResult JigResult { get; set; }

        protected string JigResultValue { get; set; }

        public FlexDrawJig(
            JigPromptOptions options,
            Func<PromptResult, Drawable> updateAction)
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

        protected override bool WorldDraw(WorldDraw draw)
        {
            return draw.Geometry.Draw(UpdateAction(JigResult));
        }
    }
}
