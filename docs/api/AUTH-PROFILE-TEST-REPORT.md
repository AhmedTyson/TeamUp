# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 01:49:52 UTC
> **Target Environment:** ASP.NET Core 10 Web API (http://localhost:5199)
> **Database Engine:** Microsoft SQL Server 2022 (Docker container 	eamup-sql:14333)
> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (DeletedAt)

---

### Test Case: 01. Service Health Check

**Request:**
```http
GET /health HTTP/1.1
Host: localhost:5199
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "status":  "Healthy"
}
```

---

### Test Case: 02. Student Registration (Happy Path)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "StrongP@ssw0rd2026!",
    "academicYear":  3,
    "lastName":  "Tyson",
    "email":  "live_student_662814197@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWUzNWIwYjAtYjlhYy00ZWViLTllMjYtNWFhODhiODVlMDk1IiwiZXhwIjoxNzkzNjY2OTk2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.9wz55E4QlsGzV70WT0kqDqdV8vWZ-tzPpmpn54DyaLU",
    "refreshToken":  "b7kbPH4C0WoMvVMSzCwjqFccNRhEUFzvCXz75knE/qyxJCS/6znFVEtLCPTUXqPbTXlUo4uuBkr9fb0t2IBL0w==",
    "expiresAt":  "2026-11-03T00:49:56.6871381Z",
    "student":  {
                    "studentId":  4,
                    "email":  "live_student_662814197@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
}
```

---

### Test Case: 03. Registration Conflict (Duplicate Email)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "AnotherPassword123!",
    "academicYear":  2,
    "lastName":  "User",
    "email":  "live_student_662814197@teamup.edu",
    "firstName":  "Duplicate",
    "facultyId":  1
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A student with this email address already exists.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-05f0fc47b1514be1e4d52caaa09d40c5-9053ac3d7dacc02a-00"
}
```

---

### Test Case: 04. Registration Validation (Empty Password)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "",
    "academicYear":  1,
    "lastName":  "Password",
    "email":  "invalid_user@teamup.edu",
    "firstName":  "Empty",
    "facultyId":  1
}
```

**Response:** `400 Bad Request` — **🟢 PASSED** (Expected: `400`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110",
    "title":  "Validation Failed",
    "status":  400,
    "detail":  "One or more validation errors occurred.",
    "instance":  "/api/v1/auth/register",
    "errors":  [
                   {
                       "field":  "password",
                       "message":  "Password is required."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must be at least 8 characters long."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one uppercase letter."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one lowercase letter."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one digit."
                   }
               ],
    "timestamp":  "2026-11-02T23:49:57.0068814Z"
}
```

---

### Test Case: 05. Registration ForeignKey Failure (Invalid Faculty ID)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "ValidPassword123!",
    "academicYear":  1,
    "lastName":  "Faculty",
    "email":  "bad_faculty_1306301106@teamup.edu",
    "firstName":  "Ghost",
    "facultyId":  99999
}
```

**Response:** `404 Not Found` — **🟢 PASSED** (Expected: `404`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title":  "Resource Not Found",
    "status":  404,
    "detail":  "Faculty with ID 99999 was not found.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-d3cab5dbe828d21fe7e3471cdd564671-5337e1a187fa1cde-00"
}
```

---

### Test Case: 06. Student Login (Valid Credentials)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "StrongP@ssw0rd2026!",
    "email":  "live_student_662814197@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMjljNTFlYzYtMGU1ZC00OGU2LWI4ZWQtYTJmMDY4M2IwZDBkIiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.eEErjGTPJdJoHfNTADLvZiTsiZcQJVtBPQebQNuLvO0",
    "refreshToken":  "2jjfR130fTn28UY8cDDvqalKu3u1Z4ybiwxctNPZsCdDlat/3Nv2I/UsWnA7pIb5rVY3HnmyoOv9/kttUhal4w==",
    "expiresAt":  "2026-11-03T00:49:57.2639856Z",
    "student":  {
                    "studentId":  4,
                    "email":  "live_student_662814197@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
}
```

---

### Test Case: 07. Login Authentication Failure (Incorrect Password)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "WrongPasswordXYZ!",
    "email":  "live_student_662814197@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-894b9213bc07564aea760dfcf40b6c17-b373950f6d032b7e-00"
}
```

---

### Test Case: 08. Login Authentication Failure (Unknown Email)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "AnyPassword123!",
    "email":  "does_not_exist@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-7684d9ee72e26ef984886ba6f08cd822-6b6edc6207ed8e32-00"
}
```

---

### Test Case: 09. Refresh Access Token (Token Rotation)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "2jjfR130fTn28UY8cDDvqalKu3u1Z4ybiwxctNPZsCdDlat/3Nv2I/UsWnA7pIb5rVY3HnmyoOv9/kttUhal4w=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0",
    "refreshToken":  "cerDWK8TvDVvx6PCjsG4tsmKYOp9QqhnlwskiO3jL37yv/01wgg9XUC3/HUoJUd3gYu+S6uOgNe5V8P6xmcxXA==",
    "expiresAt":  "2026-11-03T00:49:57.5509979Z",
    "student":  {
                    "studentId":  4,
                    "email":  "live_student_662814197@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
}
```

---

### Test Case: 10. Refresh Token Failure (Tampered/Invalid Token)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "TAMPERED_REFRESH_TOKEN_1234567890_INVALID"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-d633cd055e85196a8671643dd0a11bf3-0957c94c0a23f450-00"
}
```

---

### Test Case: 11. Protected Route Guard (Missing Authorization Header)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized",
    "status":  401,
    "traceId":  "00-49d7a2c6026abfb34499c76b54306bb4-168a05504fcf0b0c-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "studentId":  4,
    "email":  "live_student_662814197@teamup.edu",
    "firstName":  "Ahmed",
    "lastName":  "Tyson",
    "academicYear":  3,
    "role":  "Student",
    "facultyId":  1,
    "facultyName":  "Faculty of Computers and Artificial Intelligence",
    "universityName":  "Cairo University",
    "profile":  null,
    "interests":  [

                  ]
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
Content-Type: application/json

{
    "firstName":  "Ahmed (Lead)",
    "academicYear":  4,
    "lastName":  "Tyson"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "studentId":  4,
    "email":  "live_student_662814197@teamup.edu",
    "firstName":  "Ahmed (Lead)",
    "lastName":  "Tyson",
    "academicYear":  4,
    "role":  "Student",
    "facultyId":  1,
    "facultyName":  "Faculty of Computers and Artificial Intelligence",
    "universityName":  "Cairo University",
    "profile":  null,
    "interests":  [

                  ]
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/4/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  null,
    "photoUrl":  null,
    "gitHubUrl":  null,
    "linkedInUrl":  null
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
Content-Type: application/json

{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/4/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
Content-Type: application/json

{
    "interest":  "Cloud \u0026 Distributed Systems"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/4/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Artificial Intelligence",
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  2
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/4/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  1
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/4/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Artificial Intelligence",
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  2
}
```

---

### Test Case: 24. Invalidate Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNjYyODE0MTk3QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMTZmYWM3NWQtNWE1ZS00MTZhLThkNTgtMDYwMDFkMjk1NTE5IiwiZXhwIjoxNzkzNjY2OTk3LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.An2nU1dcK8E6o-fPpMU6-Njp8gm7hIh4NXb66uqSCj0
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 25. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "cerDWK8TvDVvx6PCjsG4tsmKYOp9QqhnlwskiO3jL37yv/01wgg9XUC3/HUoJUd3gYu+S6uOgNe5V8P6xmcxXA=="
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-9e950f0772606f53d8ade6ac5480aae6-643c8e79fe8e6992-00"
}
```

---

