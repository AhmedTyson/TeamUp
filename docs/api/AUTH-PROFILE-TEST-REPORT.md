# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 18:04:44 UTC
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
    "email":  "live_student_1887857836@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIwMjQ0MWU2ZS0zYzMwLTQzOWUtYmM0Zi1iNDMyYjdlZDNlNGQiLCJleHAiOjE3OTM3MjU0ODcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.BHoM1Z5cHrPWsIwP4QebviPNkUkUVDSB2Opk5AcJVbw",
                 "refreshToken":  "mLaZzhbv9nEuYvdAHW/W2vmNONn9SGMX3481eMXfM25j+/MfVwp649VttWvyx1YInjAWomewm+UFIJstdUIaEQ==",
                 "expiresAt":  "2026-11-03T17:04:47.2840685Z",
                 "student":  {
                                 "studentId":  6,
                                 "email":  "live_student_1887857836@teamup.edu",
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
    "timestamp":  "2026-11-03T16:04:47.3105708Z"
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
    "email":  "live_student_1887857836@teamup.edu",
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
    "traceId":  "00-15dd4c7653a703eaf95d0cb964a51076-b2e377756228eeef-00"
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
    "timestamp":  "2026-11-03T16:04:47.5968142Z"
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
    "email":  "bad_faculty_1573566898@teamup.edu",
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
    "traceId":  "00-e1a240dbd9377d50a7f164e56213474b-f55da9e964257693-00"
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
    "email":  "live_student_1887857836@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIyZjEwOTEwOC1iNmNjLTRlODgtYWQzOS00ZGYwY2ZlOTY3NTkiLCJleHAiOjE3OTM3MjU0ODcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.4LNbqv8DMayFLgXeaXVugCt4rOdyD8nHEbuVXK_58_M",
                 "refreshToken":  "CXm4ewTfdRjE1R+YIkO0lLdOfLoVve5k/YHJJ+WyBz604ct8Y8q0AstYOMnBb8V+pskpx4Yz70Uo7t/58cJO0Q==",
                 "expiresAt":  "2026-11-03T17:04:47.8976397Z",
                 "student":  {
                                 "studentId":  6,
                                 "email":  "live_student_1887857836@teamup.edu",
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
    "timestamp":  "2026-11-03T16:04:47.9423421Z"
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
    "email":  "live_student_1887857836@teamup.edu"
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
    "traceId":  "00-be0ce47f973f7e00e856a89104b5bb7b-0c90cca3798f7ba7-00"
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
    "traceId":  "00-9907e03ef5f5d26517533bdebd62912b-d210ea66c80051c3-00"
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
    "refreshToken":  "CXm4ewTfdRjE1R+YIkO0lLdOfLoVve5k/YHJJ+WyBz604ct8Y8q0AstYOMnBb8V+pskpx4Yz70Uo7t/58cJO0Q=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g",
                 "refreshToken":  "XcG6NZ0nC3vVo3luJdFxad7AUqIn3XLyQPftxvGaMwwT9RxThrbxCopXzNwZYSHJkvmV7WvR+nY2GazlSbbPFw==",
                 "expiresAt":  "2026-11-03T17:04:48.2050987Z",
                 "student":  {
                                 "studentId":  6,
                                 "email":  "live_student_1887857836@teamup.edu",
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
    "timestamp":  "2026-11-03T16:04:48.2224589Z"
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
    "traceId":  "00-ab410f65dfe61e52f29f10d736bc4842-1dc9c93d3245869a-00"
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
    "traceId":  "00-d59c5be36f927576e5923900200be94c-942e783db6566716-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  6,
                 "email":  "live_student_1887857836@teamup.edu",
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
    "timestamp":  "2026-11-03T16:04:48.5060829Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
                 "studentId":  6,
                 "email":  "live_student_1887857836@teamup.edu",
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
    "timestamp":  "2026-11-03T16:04:48.6324725Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/6/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:48.711704Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:48.8705774Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/6/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:48.9237179Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.0384973Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.1029843Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/6/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.1849148Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/6/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.3130232Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.3752509Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/6/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "timestamp":  "2026-11-03T16:04:49.4265547Z"
}
```

---

### Test Case: 24. Invalidate Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2IiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTg4Nzg1NzgzNkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibGl2ZV9zdHVkZW50XzE4ODc4NTc4MzZAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNGQ4ZDcyMS1hOTM3LTQ1ZjctYWQ0NC1mZTMxNGE1YTA1OWIiLCJleHAiOjE3OTM3MjU0ODgsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.D5sBw6FCdYGZplpT2sklFbYPB-sCniItOybd1_1kA1g
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
    "refreshToken":  "XcG6NZ0nC3vVo3luJdFxad7AUqIn3XLyQPftxvGaMwwT9RxThrbxCopXzNwZYSHJkvmV7WvR+nY2GazlSbbPFw=="
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
    "traceId":  "00-99d526e74cfddef0de4d74c80aa8ece1-f2fd91f688e55ac6-00"
}
```

---

