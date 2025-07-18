namespace Intellidesk.AcadNet.Common.Enums
{
    public enum eDBObjectStatus
    {
        /// <summary>
        /// Выбирать только такие ObjectId, значения свойства 
        /// IsErased которых равно false
        /// </summary>
        NotErased,
        /// <summary>
        /// Выбирать только такие ObjectId, значения свойства 
        /// IsErased которых равно true
        /// </summary>
        Erased,
        /// <summary>
        /// Выбирать все ObjectId, не зависимо от значения
        /// свойства IsErased
        /// </summary>
        Any
    }
}