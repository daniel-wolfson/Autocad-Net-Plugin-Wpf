using ID.Api.Enums;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ID.Api.Models
{
    public class EmailTemplate
    {
        public int TemplateId;
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public PsygateCandidate Candidate { get; set; }

        public EmailTemplate(eXmlEmailTemplates templateId, string subject, string link, PsygateCandidate candidate = null)
        {
            TemplateId = (int)templateId;
            Subject = subject;
            Candidate = candidate;
            Link = link;
            Body = BuildTemplate(templateId);
        }

        public EmailTemplate(eXmlEmailTemplates templateId, string subject, string name, string content, string link)
        {
            TemplateId = (int)templateId;
            Subject = subject;
            Name = name;
            Content = content;
            Link = link;
            Body = BuildTemplate(templateId);
        }

        public string BuildTemplate(eXmlEmailTemplates templateId)
        {
            switch (templateId)
            {
                case 0:
                    return $@"<![CDATA[
                        <div dir='rtl'>
                            <p class='name-candidate'><strong>[#fname#] היקר/ה,</strong></p>
                            </ br>
                            <p class='email-content'>
                            המייל הבא נשלח אליך כחלק מתהליך המיון שלך למסלולי הערבית בחיל המודיעין. הקישור שלפניך מכיל מבחן ראשוני לבדיקת רמת הידע שלך בערבית ושאלון הבוחן את העדפות השירות שלך במסלול.עליך לענות על המבחן ועל השאלון באופן מיטבי ובמסגרת הזמן המוקצב, שכן הישגיך בשלב זה ישפיעו על המשך תהליך המיון שלך בחיל.הוראות נוספות יופיעו במבחן עצמו. 
                            <strong>עליך לענות על המבחן באופן עצמאי, ללא כל סיוע חיצוני וללא מילון. אמינות הציונים תיבדק בהמשך! </strong>
                            בכל שאלה או בירור יש לפנות למרכז השירות של מיטב -
                            מייל: Meitav @idf.gov.il טלפון: 03-738-8888 או *3529.
                            </p>
                            <p class='psw-title'>סיסמה זמנית: </p>
                            <p class='psw'><strong>[#psw#]</strong></p>
                            <p class='link'><a href='{Link}'><strong>קישור למערכת</strong></a></p>
                            </ br>
                            <p>בהצלחה!</p>
                        </div>
                    ]]>";

                case (eXmlEmailTemplates)1:
                    return $@"<![CDATA[
                        <div dir='rtl'>
                        <p class='name-candidate'><strong>הי' [#fname#]</strong></p>
                        </ br>
                        <p class='email-content'>[#content#]</p>
                        </ br>
                        <p class='link'><strong>אימייל לקשר: </strong></p>
                        <a href='{Link}'><strong>{Link}</strong></a>
                        <p>תודה,</p>
                        <p class='email-content'>[#fname#]</p>
                        </div>
                    ]]>";

                default:
                    return $@"<![CDATA[<p>[#fname#]</p>]]>";
            }

        }

        public string BuildTemplate2(int templateId, PsygateCandidate Candidate, string Link, string Content, string Name)
        {
            switch (templateId)
            {
                case 0:
                    return $@"<![CDATA[
                        <div dir='rtl'>
                            <p class='name-candidate'><strong>{Candidate.FIRST_NAME} היקר/ה,</strong></p>
                            </ br>
                            <p class='email-content'>
                            המייל הבא נשלח אליך כחלק מתהליך המיון שלך למסלולי הערבית בחיל המודיעין. הקישור שלפניך מכיל מבחן ראשוני לבדיקת רמת הידע שלך בערבית ושאלון הבוחן את העדפות השירות שלך במסלול.עליך לענות על המבחן ועל השאלון באופן מיטבי ובמסגרת הזמן המוקצב, שכן הישגיך בשלב זה ישפיעו על המשך תהליך המיון שלך בחיל.הוראות נוספות יופיעו במבחן עצמו. 
                            <strong>עליך לענות על המבחן באופן עצמאי, ללא כל סיוע חיצוני וללא מילון. אמינות הציונים תיבדק בהמשך! </strong>
                            בכל שאלה או בירור יש לפנות למרכז השירות של מיטב -
                            מייל: Meitav @idf.gov.il טלפון: 03 - 738 - 8888 או  *3529.
                            </p>
                            <p class='psw-title'>סיסמה זמנית: </p>
                            <p class='psw'><strong>{Candidate.TEMPPASS}</strong></p>
                            <p class='link'><a href='{Link}'><strong>קישור למערכת</strong></a></p>
                            </ br>
                            <p>בהצלחה!</p>
                        </div>
                    ]]>";

                case 1:
                    return $@"<![CDATA[
                        <div dir='rtl'>
                        <p class='name-candidate'><strong>הי' [#fname#]</strong></p>
                        </ br>
                        <p class='email-content'>{Content}</p>
                        </ br>
                        <p class='link'><strong>אימייל לקשר: </strong></p>
                        <a href='{Link}'><strong>{Link}</strong></a>
                        <p>תודה,</p>
                        <p class='email-content'>{Name}</p>
                        </div>
                    ]]>";

                default:
                    return $@"<![CDATA[<p>[#fname#]</p>]]>";
            }
        }

        public XmlDocument BuildTemplate(string fileName = "AmanEmailTemplate")
        {
            Assembly a = typeof(Startup).Assembly;
            Stream s = a.GetManifestResourceStream($"Psygate.WebApi.{fileName}.xml");
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(s);
            s.Close();
            return xdoc;
        }
    }
}

