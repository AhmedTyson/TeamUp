# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 21:11:42 UTC
> **Target Environment:** ASP.NET Core 10 Web API (`http://localhost:5199`)
> **Database Engine:** Microsoft SQL Server 2022 (Docker container `teamup-sql:14333`)
> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (`DeletedAt`), `BaseResponse<T>` Envelope

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
    "email":  "live_student_544579109@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMjc3NmJjYmMtM2VhOC00YjBiLWE3YmEtYjA1ZDQ2NDA2ZDYyIiwiZXhwIjoxNzkzNzM2NzA1LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.fv1Z8Mb3Avp5-WidWvgS8A0zb3xIqU7lZjd9iLFmDdI",
                 "refreshToken":  "ufbX/qZxeL/gpBMK18ts1w2+tvSmXCyh3gsEylY4kTdrnhZnpP4JwaeGVDU0o5O2FA7cIGFO49CujOfzHK7Axw==",
                 "expiresAt":  "2026-11-03T20:11:45.619068Z",
                 "student":  {
                                 "studentId":  8,
                                 "email":  "live_student_544579109@teamup.edu",
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Student registered successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:11:45.6458397Z"
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
    "email":  "live_student_544579109@teamup.edu",
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
    "traceId":  "00-fde16980e18fd2bdfed8c558aaeb56a3-f0824fab4f01ce64-00"
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
    "timestamp":  "2026-11-03T19:11:45.9323268Z"
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
    "email":  "bad_faculty_328988665@teamup.edu",
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
    "traceId":  "00-1ba9870f15c72c688fe3a67792f03118-0dfd4d5d0e96b841-00"
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
    "email":  "live_student_544579109@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiNmU0ZjAyZDMtMTBiYy00OGM2LTg2OWMtOTAwZTZmOTQyMjc1IiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.fhGC2DkjQqQsx5MxUKd64Uxoyzn0Jb78n3idu7RrVR4",
                 "refreshToken":  "7FjVKjvStWI0y1ycqt2U+a+O5zAH0tRfRvibkKicGCOnovI+zYCwy4Lb3+T1qqMaMd4pbl0+St46Qt+REONs2g==",
                 "expiresAt":  "2026-11-03T20:11:46.246864Z",
                 "student":  {
                                 "studentId":  8,
                                 "email":  "live_student_544579109@teamup.edu",
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:46.2967851Z"
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
    "email":  "live_student_544579109@teamup.edu"
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
    "traceId":  "00-f775afe5a4e0a94f2249dd2970806615-af7f46bc5bb03bd1-00"
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
    "traceId":  "00-1b3a666221215ffcb812197f028c08b1-46c205e5f7e05cab-00"
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
    "refreshToken":  "7FjVKjvStWI0y1ycqt2U+a+O5zAH0tRfRvibkKicGCOnovI+zYCwy4Lb3+T1qqMaMd4pbl0+St46Qt+REONs2g=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88",
                 "refreshToken":  "qRKV19yoDj576HBGhryoVhHdku8YeeSltVr4QvZmck5iGY/Nuxin/jLdUHj7MdovDiBZn5mBRi7wtQU9geNFDQ==",
                 "expiresAt":  "2026-11-03T20:11:46.5452358Z",
                 "student":  {
                                 "studentId":  8,
                                 "email":  "live_student_544579109@teamup.edu",
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Token refreshed successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:46.5542108Z"
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
    "traceId":  "00-bdfa7021af6cdd0b31685b5c6845a5c9-764a48c6fa11084b-00"
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
    "traceId":  "00-d5f532381090afa4ef032e740a9e33cc-d6557f82321f0e3b-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  8,
                 "email":  "live_student_544579109@teamup.edu",
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
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:46.8458962Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
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
    "data":  {
                 "studentId":  8,
                 "email":  "live_student_544579109@teamup.edu",
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
             },
    "success":  true,
    "message":  "Student account updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:46.9699221Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/8/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "bio":  null,
                 "photoUrl":  null,
                 "gitHubUrl":  null,
                 "linkedInUrl":  null
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.0724283Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
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
    "data":  {
                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                 "gitHubUrl":  "https://github.com/AhmedTyson",
                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
             },
    "success":  true,
    "message":  "Profile updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.208117Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/8/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                 "gitHubUrl":  "https://github.com/AhmedTyson",
                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.2463105Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.3506614Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
Content-Type: application/json

{
    "interest":  "Cloud \u0026 Distributed Systems"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.39823Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/8/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Artificial Intelligence",
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.4637527Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/8/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.5724126Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.6232618Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/8/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Artificial Intelligence",
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:47.6621455Z"
}
```

---

### Test Case: 24. PlatformAdmin Authentication

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "Adm!0RK8zvFIO2BDTX",
    "email":  "admin@teamup.local"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw",
                 "refreshToken":  "RLRJ6nxSgZwVaWzIhoJLjg3EPwNzn0C04oUTK7Rt0qhOKW9GnMVx10iT0gXtgS7H0dVEGcMBXEnnG1iqeEHQeg==",
                 "expiresAt":  "2026-11-03T20:11:49.8869931Z",
                 "student":  {
                                 "studentId":  1,
                                 "email":  "admin@teamup.local",
                                 "firstName":  "Platform",
                                 "lastName":  "Admin",
                                 "academicYear":  1,
                                 "role":  "PlatformAdmin",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:49.9057623Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [
                               {
                                   "universityId":  1,
                                   "name":  "Cairo University",
                                   "city":  "Giza",
                                   "country":  "Egypt",
                                   "website":  "https://cu.edu.eg",
                                   "facultyCount":  2
                               }
                           ],
                 "page":  1,
                 "pageSize":  10,
                 "totalCount":  1,
                 "totalPages":  1
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:50.0559433Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  1,
                 "name":  "Cairo University",
                 "city":  "Giza",
                 "country":  "Egypt",
                 "website":  "https://cu.edu.eg",
                 "faculties":  [
                                   {
                                       "facultyId":  1,
                                       "name":  "Faculty of Computers and Artificial Intelligence",
                                       "code":  "FCAI"
                                   },
                                   {
                                       "facultyId":  2,
                                       "name":  "Faculty of Engineering",
                                       "code":  "ENG"
                                   }
                               ]
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:50.1484501Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Student Unauthorized University",
    "city":  "Cairo"
}
```

**Response:** `403 Forbidden` — **🟢 PASSED** (Expected: `403`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.4",
    "title":  "Forbidden",
    "status":  403,
    "traceId":  "00-813c6c7fdfe271ccf4d9355554522ca7-953cf63d62805774-00"
}
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 910023581",
    "website":  "https://alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "universityId":  3,
                 "name":  "Alexandria University 910023581",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:11:50.2712009Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 910023581",
    "city":  "Alexandria"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A university named \u0027Alexandria University 910023581\u0027 already exists.",
    "instance":  "/api/v1/universities",
    "traceId":  "00-64348e570e0323133a926254345480f8-4706a7d509e59e99-00"
}
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/3 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 910023581 Updated",
    "website":  "https://updated.alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  3,
                 "name":  "Alexandria University 910023581 Updated",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://updated.alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:50.4291918Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/3/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [

             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:50.5370172Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/3/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "code":  "MED_ALEX",
    "name":  "Faculty of Medicine"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "facultyId":  4,
                 "name":  "Faculty of Medicine",
                 "code":  "MED_ALEX",
                 "universityId":  3,
                 "universityName":  "Alexandria University 910023581 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:11:50.6483348Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/3/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "code":  "MED_ALEX",
    "name":  "Faculty of Medicine Duplicate"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A faculty with code \u0027MED_ALEX\u0027 already exists in Alexandria University 910023581 Updated.",
    "instance":  "/api/v1/universities/3/faculties",
    "traceId":  "00-8e9aed08727b4114f393974524747d66-7d63a1097e10bc33-00"
}
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/4 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
Content-Type: application/json

{
    "code":  "MED_HEALTH",
    "name":  "Faculty of Medicine \u0026 Health"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "facultyId":  4,
                 "name":  "Faculty of Medicine \u0026 Health",
                 "code":  "MED_HEALTH",
                 "universityId":  3,
                 "universityName":  "Alexandria University 910023581 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:50.7808341Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/4 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/3 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 37. Search Student Directory (GET /students?search=...)

**Request:**
```http
GET /api/v1/students?search=Ahmed&page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [
                               {
                                   "studentId":  2,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_148184769@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  3,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_17361202@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  4,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_662814197@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  5,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_664792274@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  6,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_1887857836@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  7,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_849246869@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  8,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_544579109@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               }
                           ],
                 "page":  1,
                 "pageSize":  10,
                 "totalCount":  7,
                 "totalPages":  1
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.0721108Z"
}
```

---

### Test Case: 38. Get Public Student Profile Card (GET /students/{id})

**Request:**
```http
GET /api/v1/students/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiM2VjOTFhODEtOTk2Yi00ZTU5LWIwMzItNTQzODE0MDM2YTRkIiwiZXhwIjoxNzkzNzM2NzA2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.TaKlfpVQT-9gwikGj5z3pJZhM60Fqh8xctDDDuEJt88
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  8,
                 "firstName":  "Ahmed (Lead)",
                 "lastName":  "Tyson",
                 "academicYear":  4,
                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                 "universityName":  "Cairo University",
                 "profile":  {
                                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                                 "gitHubUrl":  "https://github.com/AhmedTyson",
                                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
                             },
                 "interests":  [
                                   "Artificial Intelligence",
                                   "Cloud \u0026 Distributed Systems"
                               ]
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.1269016Z"
}
```

---

### Test Case: 39. Request Password Reset Token (POST /auth/forgot-password)

**Request:**
```http
POST /api/v1/auth/forgot-password HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "email":  "live_student_544579109@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "message":  "If your email is registered, a password reset link has been dispatched.",
                 "resetToken":  "94kXRd4YcPsohFLqzzvEsxaffc3t6WAF9unjtZh4ioKdD8YTJ2sedCldmPb5Fmm0eD3CThejV/MNjI04heZkrA=="
             },
    "success":  true,
    "message":  "Password reset request processed.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.2448937Z"
}
```

---

### Test Case: 40. Reset Password with Token (POST /auth/reset-password)

**Request:**
```http
POST /api/v1/auth/reset-password HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "email":  "live_student_544579109@teamup.edu",
    "newPassword":  "UpdatedSecurePassword2026!",
    "token":  "94kXRd4YcPsohFLqzzvEsxaffc3t6WAF9unjtZh4ioKdD8YTJ2sedCldmPb5Fmm0eD3CThejV/MNjI04heZkrA=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Password reset successfully. You can now login with your new credentials.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.4050895Z"
}
```

---

### Test Case: 41. Authenticate with New Password (POST /auth/login)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "UpdatedSecurePassword2026!",
    "email":  "live_student_544579109@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgKExlYWQpIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImEzNGU2YTZhLWIwNjAtNDY4NS1iY2Y1LWM0M2JhODJmNTgwZCIsImV4cCI6MTc5MzczNjcxMSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bg0PuPO-4rKhfLp9QnsHgMXSV3SDb1RkWswOpxtVAZw",
                 "refreshToken":  "UdQD3keeRy5amDxGaVaIklLuLJjLZ+KhMFp+vUbGbjhIdMn9vDkPjwobddwXAO1bFfunb1mxdP9RwPCwXt1N2w==",
                 "expiresAt":  "2026-11-03T20:11:51.5499755Z",
                 "student":  {
                                 "studentId":  8,
                                 "email":  "live_student_544579109@teamup.edu",
                                 "firstName":  "Ahmed (Lead)",
                                 "lastName":  "Tyson",
                                 "academicYear":  4,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.5590747Z"
}
```

---

### Test Case: 42. Soft-Delete Student Account (DELETE /students/me)

**Request:**
```http
DELETE /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI4IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfNTQ0NTc5MTA5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgKExlYWQpIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImEzNGU2YTZhLWIwNjAtNDY4NS1iY2Y1LWM0M2JhODJmNTgwZCIsImV4cCI6MTc5MzczNjcxMSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bg0PuPO-4rKhfLp9QnsHgMXSV3SDb1RkWswOpxtVAZw
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 43. Login Rejection for Soft-Deleted Student

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "UpdatedSecurePassword2026!",
    "email":  "live_student_544579109@teamup.edu"
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
    "traceId":  "00-898c493b064156f030d258d9ea3993e3-4b79796af94439fd-00"
}
```

---

### Test Case: 44. Verify Soft-Deleted Student Omitted from Directory

**Request:**
```http
GET /api/v1/students?search=live_student_544579109@teamup.edu HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [

                           ],
                 "page":  1,
                 "pageSize":  20,
                 "totalCount":  0,
                 "totalPages":  0
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:11:51.6901943Z"
}
```

---

### Test Case: 45. Invalidate Admin Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2NhZjMzYmYtYjZiMi00NmFlLTliYTUtNmIxZmRlYjE3ZmEwIiwiZXhwIjoxNzkzNzM2NzA5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WOw6JLltxMIRuX46Emaj9qbWNlPTiE6vnvrAa2RSKFw
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 46. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "qRKV19yoDj576HBGhryoVhHdku8YeeSltVr4QvZmck5iGY/Nuxin/jLdUHj7MdovDiBZn5mBRi7wtQU9geNFDQ=="
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
    "traceId":  "00-2f36e20fcf6d19e46893b623c4c458bc-de30d26c0afde94c-00"
}
```

---

