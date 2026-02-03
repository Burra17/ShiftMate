using System.Collections.Generic;
﻿
﻿namespace ShiftMate.Domain
﻿{
﻿    public class User
﻿    {
﻿        public Guid Id { get; set; }
﻿                public string FirstName { get; set; } = string.Empty;
﻿                public string LastName { get; set; } = string.Empty;
﻿                public string Email { get; set; } = string.Empty;
﻿                public string PasswordHash { get; set; } = string.Empty;﻿        public Role Role { get; set; }
﻿
﻿        // Navigation Properties (Hjälper EF Core att koppla ihop tabeller)
﻿        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
﻿
﻿        // Relationer för bytesförfrågningar
﻿        public virtual ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();
﻿        public virtual ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
﻿    }
﻿
﻿    public enum Role
﻿    {
﻿        Employee,
﻿        Manager,
﻿        Admin
﻿    }
﻿}
﻿