# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 18:38:55 UTC
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
    "email":  "live_student_849246869@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiNzlmYTBkZjYtNTViZC00YTgzLWE5NDItZjI2Yzk5NWQ5NDY2IiwiZXhwIjoxNzkzNzI3NTM4LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.WEcFyBuA7PHCK8OcHaGP9S69graAFPKpRkcNPFvBKug",
                 "refreshToken":  "JrzNAuq/VhCcswJLxJxoJPfAj/oTZCtbnZlf5k9Ksw2LrJlV61mn8wxaqqxgKC8A5cUfcSRbUYlzSwcc7Z8XrQ==",
                 "expiresAt":  "2026-11-03T17:38:58.8163261Z",
                 "student":  {
                                 "studentId":  7,
                                 "email":  "live_student_849246869@teamup.edu",
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
    "timestamp":  "2026-11-03T16:38:58.8425061Z"
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
    "email":  "live_student_849246869@teamup.edu",
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
    "traceId":  "00-b8199f5a25c3d58b2fb12bb1be1bd1bc-ab4e023f4030033c-00"
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
    "timestamp":  "2026-11-03T16:38:59.1169941Z"
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
    "email":  "bad_faculty_2147103178@teamup.edu",
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
    "traceId":  "00-2bcf759d0685c691a4d624382c189fad-6cca1b2eca154c46-00"
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
    "email":  "live_student_849246869@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiNWMwMDY3NTAtZmYzZS00M2YxLTk4ZmQtYjg5OTQ2Y2M2Y2I4IiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.FK8zsHHFcBs8w7vq7k9O7DPY8rmtquZVMkgniv6ULp4",
                 "refreshToken":  "ni6rLTwsqiEUcsv5k2flnmvhih9uAOLnhCS6aigjCmhWUd95HgqphmRacOIB0uY11bPSkIWgy8yh7BYsnPyepA==",
                 "expiresAt":  "2026-11-03T17:38:59.4163073Z",
                 "student":  {
                                 "studentId":  7,
                                 "email":  "live_student_849246869@teamup.edu",
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
    "timestamp":  "2026-11-03T16:38:59.4614372Z"
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
    "email":  "live_student_849246869@teamup.edu"
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
    "traceId":  "00-4a374b965a576a079e18e8d383887a7c-8e42e1f1a3ca294a-00"
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
    "traceId":  "00-6ff3bed84faf085720905a62ed514447-b400631ed9ae6c12-00"
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
    "refreshToken":  "ni6rLTwsqiEUcsv5k2flnmvhih9uAOLnhCS6aigjCmhWUd95HgqphmRacOIB0uY11bPSkIWgy8yh7BYsnPyepA=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4",
                 "refreshToken":  "vNplxo6oomNNHNQo2IooQLX/CXXXhOVFDxxjpjwfBhVvmm2N5jpnp39PR46BC285U1uHFhvgiZPhuix6DCAnOw==",
                 "expiresAt":  "2026-11-03T17:38:59.7078347Z",
                 "student":  {
                                 "studentId":  7,
                                 "email":  "live_student_849246869@teamup.edu",
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
    "timestamp":  "2026-11-03T16:38:59.7249893Z"
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
    "traceId":  "00-334611904f68f52fd36c00226666c320-0e97f3bae70d347a-00"
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
    "traceId":  "00-89c14227258f3bda2e68a47982eaa1bc-de7e97d46f3c1981-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  7,
                 "email":  "live_student_849246869@teamup.edu",
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
    "timestamp":  "2026-11-03T16:39:00.0140452Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
                 "studentId":  7,
                 "email":  "live_student_849246869@teamup.edu",
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
    "timestamp":  "2026-11-03T16:39:00.1563201Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/7/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.2374857Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.4207281Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/7/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.4752029Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.5933826Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.6546612Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/7/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.7347795Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/7/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.8671075Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.9298283Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/7/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:00.9807652Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM",
                 "refreshToken":  "OSsSKalsJrQFHBSClPQStdE+jbZ80EYhD1OZJwTmP87PnJQrwCkj8dWih6TlJbSAdG1WvFL+2jSqiUF/81TcyA==",
                 "expiresAt":  "2026-11-03T17:39:03.1551951Z",
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
    "timestamp":  "2026-11-03T16:39:03.1675633Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:03.3541629Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "timestamp":  "2026-11-03T16:39:03.4655831Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
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
    "traceId":  "00-82f6ad8370b44139384683c41e11708e-2190cfc943ffcfd8-00"
}
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 1128166698",
    "website":  "https://alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "universityId":  2,
                 "name":  "Alexandria University 1128166698",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T16:39:03.6358138Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 1128166698",
    "city":  "Alexandria"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A university named \u0027Alexandria University 1128166698\u0027 already exists.",
    "instance":  "/api/v1/universities",
    "traceId":  "00-2105312c7b79657f2468233ffe4b6bc6-6fd0bc71bc845f70-00"
}
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/2 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 1128166698 Updated",
    "website":  "https://updated.alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  2,
                 "name":  "Alexandria University 1128166698 Updated",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://updated.alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T16:39:03.8234343Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/2/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [

             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T16:39:03.9463854Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/2/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
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
                 "facultyId":  3,
                 "name":  "Faculty of Medicine",
                 "code":  "MED_ALEX",
                 "universityId":  2,
                 "universityName":  "Alexandria University 1128166698 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T16:39:04.0808766Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/2/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
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
    "detail":  "A faculty with code \u0027MED_ALEX\u0027 already exists in Alexandria University 1128166698 Updated.",
    "instance":  "/api/v1/universities/2/faculties",
    "traceId":  "00-16f665eabf4ae5c1a0b2a125a852ab30-c4f59d5a6926a1df-00"
}
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/3 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
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
                 "facultyId":  3,
                 "name":  "Faculty of Medicine \u0026 Health",
                 "code":  "MED_HEALTH",
                 "universityId":  2,
                 "universityName":  "Alexandria University 1128166698 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T16:39:04.2353406Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/3 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/2 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNTVlYTNlMDgtNmY5My00YTc3LTlhYzctZjJkOTU5ZmEyYzgyIiwiZXhwIjoxNzkzNzI3NTQzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.6p-nBv6YJ-2AkthGGmat6LH-Bu-IuRhXOO74zR6okFM
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 37. Invalidate Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODQ5MjQ2ODY5QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMzEzNzdmODEtM2NlMS00M2U0LThmMDItMjNmYjIwMjg2ZDVlIiwiZXhwIjoxNzkzNzI3NTM5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.gkqcLsZyOt6zmNqOMnHsEoBU0_yV3NGEdjC-B9wXmQ4
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 38. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "vNplxo6oomNNHNQo2IooQLX/CXXXhOVFDxxjpjwfBhVvmm2N5jpnp39PR46BC285U1uHFhvgiZPhuix6DCAnOw=="
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
    "traceId":  "00-9f59f85d42b2137b937e4c69d1e77499-d009304035224740-00"
}
```

---

