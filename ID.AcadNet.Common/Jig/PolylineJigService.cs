using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class PolylineJigService
    {
        internal Document Document => Application.DocumentManager.MdiActiveDocument;
        internal Editor Editor => Document.Editor;

        public Polyline Draw(bool closable, bool undoable, short colorIndex)
        {
            if (closable && !undoable)
                return DrawClosable(colorIndex);
            if (!closable && undoable)
                return DrawUndoable(colorIndex);
            if (closable)
                return DrawClosableUndoable(colorIndex);

            return Draw(colorIndex);
        }

        private Polyline Draw(short colorIndex)
        {
            PolylineJig jig;

            using (jig = new PolylineJig(colorIndex))
            {
                // Loop to set the vertices directly on the polyline
                bool bSuccess, bComplete;

                do
                {
                    PromptResult res = Editor.Drag(jig);

                    bSuccess = res.Status == PromptStatus.OK;
                    bComplete = res.Status == PromptStatus.None;

                    if (bSuccess)
                    {
                        jig.Draw(colorIndex);
                    }

                } while (bSuccess && !bComplete);

            }
            return jig.Polyline;
        }

        private Polyline DrawClosable(short colorIndex)
        {
            PolylineJig jig;

            var collection = new KeywordCollection { { "C", "C", "Close", true, true } };

            using (jig = new PolylineJig("Select point of polyline", collection, colorIndex))
            {
                // Loop to set the vertices directly on the polyline
                bool bSuccess, bComplete, bKeyword, bClosed = false;

                do
                {
                    PromptResult res = Editor.Drag(jig);

                    jig.Prompt = "Draw next point of polyline";

                    bSuccess = res.Status == PromptStatus.OK;
                    bKeyword = res.Status == PromptStatus.Keyword;
                    bComplete = res.Status == PromptStatus.None;

                    if (bSuccess && !bKeyword)
                    {
                        jig.Draw(colorIndex);
                    }

                    if (bKeyword)
                    {
                        if (res.StringResult == "C")
                        {
                            bClosed = true;
                            bComplete = true;
                            jig.Close();
                        }

                    }
                } while (!bSuccess && bKeyword && !bClosed || bSuccess && !bComplete);

            }
            return jig.Polyline;
        }

        private Polyline DrawUndoable(short colorIndex)
        {
            var collection = new KeywordCollection { { "U", "U", "Undo", true, true } };
            using (PolylineJig jig = new PolylineJig("Select point of polyline", collection, colorIndex))
            {
                // Loop to set the vertices directly on the polyline
                bool bSuccess, bComplete, bKeyword;

                do
                {
                    PromptResult res = Editor.Drag(jig);

                    bSuccess = res.Status == PromptStatus.OK;
                    bKeyword = res.Status == PromptStatus.Keyword;
                    bComplete = res.Status == PromptStatus.None;

                    if (bSuccess && !bKeyword)
                    {
                        jig.Draw(colorIndex);
                    }

                    if (bKeyword)
                    {
                        if (res.StringResult == "U")
                            jig.Undo();

                    }
                } while (!bSuccess && bKeyword || bSuccess && !bComplete);

                if (bComplete)
                    return jig.Polyline;
            }
            return null;
        }

        private Polyline DrawClosableUndoable(short colorIndex)
        {
            PolylineJig jig;

            var collection = new KeywordCollection { { "C", "C", "Close", true, true }, { "U", "U", "Undo", true, true } };

            using (jig = new PolylineJig("Select point of polyline", collection, colorIndex))
            {
                // Loop to set the vertices directly on the polyline
                bool bSuccess, bComplete, bKeyword, bClosed = false;

                do
                {
                    PromptResult res = Editor.Drag(jig);

                    bSuccess = res.Status == PromptStatus.OK;
                    bKeyword = res.Status == PromptStatus.Keyword;
                    bComplete = res.Status == PromptStatus.None;

                    if (bSuccess && !bKeyword)
                    {
                        jig.Draw(colorIndex);
                    }

                    if (bKeyword)
                    {
                        if (res.StringResult == "U")
                            jig.Undo();
                        if (res.StringResult == "C")
                        {
                            bClosed = true;
                            bComplete = true;
                            jig.Close();
                        }
                    }
                } while (!bSuccess && bKeyword && !bClosed || bSuccess && !bComplete);

            }
            return jig.Polyline;
        }
    }
}