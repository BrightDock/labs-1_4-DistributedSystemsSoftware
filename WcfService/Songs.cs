//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfService
{
    using System;
    using System.Collections.Generic;
    
    public partial class Songs
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Songs()
        {
            this.Songs_descriptions = new HashSet<Songs_descriptions>();
        }
    
        public long Id { get; set; }
        public long Id_author { get; set; }
        public string Name { get; set; }
        public Nullable<System.TimeSpan> Length { get; set; }
        public Nullable<short> Year { get; set; }
        public string Path_location { get; set; }
    
        public virtual Authors Authors { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Songs_descriptions> Songs_descriptions { get; set; }
    }
}
