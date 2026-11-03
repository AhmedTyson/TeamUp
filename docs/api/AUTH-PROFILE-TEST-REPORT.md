# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 21:33:42 UTC
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
    "email":  "live_student_885254441@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiZmExYWMzYjYtMmU1Ny00ODI3LWJlNWEtZGM4OTNkYWE3MmE4IiwiZXhwIjoxNzkzNzM4MDI1LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.DIMZr9mmLbSqbaeFVFdFChtvMgyP2J3GebXo1V0duF8",
                 "refreshToken":  "4ycZCBxBLRdRCgdmzNdgVBnQObxBkiu7K+Qm9/4TarrdRtEizj24WLpOvHNJOc06EZnyA8zxRtiOwfY99BYC8g==",
                 "expiresAt":  "2026-11-03T20:33:45.3848321Z",
                 "student":  {
                                 "studentId":  9,
                                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:45.4128488Z"
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
    "email":  "live_student_885254441@teamup.edu",
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
    "traceId":  "00-9b4651a879a8cfde03daff45299227a5-9cc5f413b38ade9e-00"
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
    "timestamp":  "2026-11-03T19:33:45.6942434Z"
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
    "email":  "bad_faculty_691028526@teamup.edu",
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
    "traceId":  "00-d97ccefa3bfd7357ed4e27b337fe6357-a64c3113c3fe472f-00"
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
    "email":  "live_student_885254441@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMjYxZTZkM2MtZmM5NC00NDQxLTlmZDMtYjUzMTk2MWM1M2M1IiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.Y4s69wUpp_e9dwjVavLCsENxXCUc3p205Y1W4GdIZuQ",
                 "refreshToken":  "Hi8nUrfdzHL2jPu9NYEcM4lF/vTfPF8VpXHwYsPWuP/7aIJsPi4jzZjk2a2LmjIOItc5mpCM1yOK+bVbEw3aRQ==",
                 "expiresAt":  "2026-11-03T20:33:46.0082629Z",
                 "student":  {
                                 "studentId":  9,
                                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:46.0446985Z"
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
    "email":  "live_student_885254441@teamup.edu"
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
    "traceId":  "00-9ee7cbbc96065320ef96927738132c8c-70870e84bfb37b5b-00"
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
    "traceId":  "00-1f997a733b24d865776cc90921019a53-dc7c0b9a6cf26433-00"
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
    "refreshToken":  "Hi8nUrfdzHL2jPu9NYEcM4lF/vTfPF8VpXHwYsPWuP/7aIJsPi4jzZjk2a2LmjIOItc5mpCM1yOK+bVbEw3aRQ=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ",
                 "refreshToken":  "9kwLhwsCWoyb6tk7I7tqoMWACB1YoNyrfYzWpLh6vqLuyivB9N6gloOEglQxkgTy+Wp6yfgRQZ4dLHb3H2yH6g==",
                 "expiresAt":  "2026-11-03T20:33:46.3040483Z",
                 "student":  {
                                 "studentId":  9,
                                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:46.3160185Z"
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
    "traceId":  "00-c70dfc2763db919155742e55c147c5d3-0bfa7f3332d573e3-00"
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
    "traceId":  "00-e015ef416fb555d2cd9801932b0bb942-d57ba09ba1cd9c0b-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  9,
                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:46.6310454Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
                 "studentId":  9,
                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:46.7562302Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/9/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:46.8338628Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:46.9785492Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/9/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.0302945Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.140861Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.2013622Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/9/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.2811664Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/9/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.408588Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.4700568Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/9/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:47.5191917Z"
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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k",
                 "refreshToken":  "SY+qxXyTF64+mkkGWUpt9NHZ4cRTUb3n+kOjy/Gih748/6kq6PPKLBmLHlNY3yZB7QF/yOD7OaQh7GnCYqQwDw==",
                 "expiresAt":  "2026-11-03T20:33:49.7192615Z",
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
    "timestamp":  "2026-11-03T19:33:49.7431308Z"
}
```

---

### Test Case: 24B. Public Academic Catalog Lookup for Registration (GET /universities/catalog)

**Request:**
```http
GET /api/v1/universities/catalog HTTP/1.1
Host: localhost:5199
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 {
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
                 }
             ],
    "success":  true,
    "message":  "Academic catalog retrieved successfully for registration lookups.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:49.8811705Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:50.0358067Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "timestamp":  "2026-11-03T19:33:50.140397Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
    "traceId":  "00-e8214f6fcb42e6b2e436259b2d563df2-9e76fecc3f2054b1-00"
}
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 983230740",
    "website":  "https://alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "universityId":  4,
                 "name":  "Alexandria University 983230740",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:33:50.259636Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 983230740",
    "city":  "Alexandria"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A university named \u0027Alexandria University 983230740\u0027 already exists.",
    "instance":  "/api/v1/universities",
    "traceId":  "00-5c8c02ac16732154a714a57c5db046c2-ee6068f35ac0c221-00"
}
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/4 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 983230740 Updated",
    "website":  "https://updated.alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  4,
                 "name":  "Alexandria University 983230740 Updated",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://updated.alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:50.4285599Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/4/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [

             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:50.551308Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/4/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
                 "facultyId":  5,
                 "name":  "Faculty of Medicine",
                 "code":  "MED_ALEX",
                 "universityId":  4,
                 "universityName":  "Alexandria University 983230740 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:33:50.6625479Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/4/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
    "detail":  "A faculty with code \u0027MED_ALEX\u0027 already exists in Alexandria University 983230740 Updated.",
    "instance":  "/api/v1/universities/4/faculties",
    "traceId":  "00-b2ebeafa54e1614fd71bd4beac8f0b48-d05068b54706aaee-00"
}
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/5 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
                 "facultyId":  5,
                 "name":  "Faculty of Medicine \u0026 Health",
                 "code":  "MED_HEALTH",
                 "universityId":  4,
                 "universityName":  "Alexandria University 983230740 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:50.810957Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/5 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/4 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
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
                                   "studentId":  9,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:51.0788979Z"
}
```

---

### Test Case: 38. Get Public Student Profile Card (GET /students/{id})

**Request:**
```http
GET /api/v1/students/9 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiOWE3ODBiNTgtNzI0Yy00YWRjLTg5NDktNjY1ZDY3YTI0MTMzIiwiZXhwIjoxNzkzNzM4MDI2LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.tW56oCCrvc5YIqZ4M1c5qW5mEBfAOeGWsNg5cj5KQlQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  9,
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
    "timestamp":  "2026-11-03T19:33:51.1480801Z"
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
    "email":  "live_student_885254441@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "message":  "If your email is registered, a password reset link has been dispatched.",
                 "resetToken":  "H8DdibUHKsrELqIMDvGz5uVj3s26zsHfeV0DffCRl8CXQ6RdfGkm71z6IOWkVhaEmzL+IPz/dhc4jBF0o5em1A=="
             },
    "success":  true,
    "message":  "Password reset request processed.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:51.2485297Z"
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
    "email":  "live_student_885254441@teamup.edu",
    "newPassword":  "UpdatedSecurePassword2026!",
    "token":  "H8DdibUHKsrELqIMDvGz5uVj3s26zsHfeV0DffCRl8CXQ6RdfGkm71z6IOWkVhaEmzL+IPz/dhc4jBF0o5em1A=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Password reset successfully. You can now login with your new credentials.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:33:51.4252203Z"
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
    "email":  "live_student_885254441@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgKExlYWQpIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjUzNGM2NjdlLTdiZWUtNDhmNS1hMDQ0LTdjNzQ3NThjNTU3YyIsImV4cCI6MTc5MzczODAzMSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.WCKcaCRX7MeNkI0rnk0RnccSuiTTUzDI0sWwmMfwU7I",
                 "refreshToken":  "0jCTeOKloDqjI1CaXWc+0H5y16RNiQqhA0ja4sV++LmjxIm0FiB3A1RNozEuF7Ess7ec98NCHYnze4GeivWoOQ==",
                 "expiresAt":  "2026-11-03T20:33:51.585095Z",
                 "student":  {
                                 "studentId":  9,
                                 "email":  "live_student_885254441@teamup.edu",
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
    "timestamp":  "2026-11-03T19:33:51.5946975Z"
}
```

---

### Test Case: 42. Soft-Delete Student Account (DELETE /students/me)

**Request:**
```http
DELETE /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI5IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfODg1MjU0NDQxQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWhtZWQgKExlYWQpIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjUzNGM2NjdlLTdiZWUtNDhmNS1hMDQ0LTdjNzQ3NThjNTU3YyIsImV4cCI6MTc5MzczODAzMSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.WCKcaCRX7MeNkI0rnk0RnccSuiTTUzDI0sWwmMfwU7I
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
    "email":  "live_student_885254441@teamup.edu"
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
    "traceId":  "00-8dca899d13535aef8139b9aa18187aef-935b218cabd8c981-00"
}
```

---

### Test Case: 44. Verify Soft-Deleted Student Omitted from Directory

**Request:**
```http
GET /api/v1/students?search=live_student_885254441@teamup.edu HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
    "timestamp":  "2026-11-03T19:33:51.7372879Z"
}
```

---

### Test Case: 45. Invalidate Admin Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNWM5OGJjYTUtMzA0Yi00NzgyLTk3MDktNzYxYzY1MmY3ODI3IiwiZXhwIjoxNzkzNzM4MDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.AZB_jN3yx2DiOfX5r0GNk0LpeXctewuf4sdJtl-Kk2k
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
    "refreshToken":  "9kwLhwsCWoyb6tk7I7tqoMWACB1YoNyrfYzWpLh6vqLuyivB9N6gloOEglQxkgTy+Wp6yfgRQZ4dLHb3H2yH6g=="
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
    "traceId":  "00-276b673f3265501988bc35838a23c4fe-de229aacd0e9122a-00"
}
```

---

