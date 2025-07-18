namespace ID.Infrastructure.Enums
{
    public enum UserPermissionTypes
    {
        SystemAdmin = 1,
        OrganizationalAdmin = 2,
        RegularUser = 3
    }

    public enum EvFormType
    {
        Evaluation = 1,
        Sociometric = 2
    }
    public enum EvChapterType
    {
        Tabular = 1,
        Textual = 2,
        Introduction = 3,
        RadioButtons = 4,
    }

    public enum EvStatus
    {
        Preparation = 1,
        Execution = 2,
        Closure = 3,
        MoveToRepository = 4,
        Paused = 5
    }

    public enum EvaluationsStatisticStatusType
    {
        ApprovedForm = 1,
        ExceptionalForm = 2,
        DisqualifiedFormByAdmin = 3
    }
    public enum Status
    {
        Draft = 0,
        Active = 1,
        NotActive = 2
    }

    public enum EventContactType
    {
        EvaluationModel = 1,
        MapModel = 2,
        LocatingEvaluators = 3,
    }

    public enum ReasonNotSendingDataForEventExecution
    {
        EventNotFullySaved = 1,
        MissingWhoIsEvaluatingWho = 2,
        ImproperlyConstructedEventForm = 3,
        MissingMetaDataUnitsOrRanks = 4,
        MissingConnectQuestionToTreeSubject = 5
    }

    public enum GenderEnum
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }


    public enum UserTypes
    {
        User = 1,
        HR = 2,
        UserHR = 1,
    }
}
