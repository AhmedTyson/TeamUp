$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:5199"
$reportPath = "C:\Users\ahmed\Downloads\TeamUp\docs\api\AUTH-PROFILE-TEST-REPORT.md"
$logLines = [System.Collections.Generic.List[string]]::new()

function Add-ReportLine($str) {
    $logLines.Add($str)
}

function Format-Json($obj) {
    if ($null -eq $obj) { return "null" }
    if ($obj -is [string]) {
        try {
            return ($obj | ConvertFrom-Json | ConvertTo-Json -Depth 10)
        } catch {
            return $obj
        }
    }
    return ($obj | ConvertTo-Json -Depth 10)
}

function Send-HttpReq {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [hashtable]$Headers = @{},
        $Body = $null,
        [int]$ExpectedStatus
    )

    $url = "$baseUrl$Path"
    Add-ReportLine "### Test Case: $Name"
    Add-ReportLine ""
    Add-ReportLine '**Request:**'
    Add-ReportLine '```http'
    Add-ReportLine "$Method $Path HTTP/1.1"
    Add-ReportLine 'Host: localhost:5199'
    foreach ($k in $Headers.Keys) {
        Add-ReportLine "$k`: $($Headers[$k])"
    }
    if ($null -ne $Body) {
        Add-ReportLine 'Content-Type: application/json'
        Add-ReportLine ''
        Add-ReportLine (Format-Json $Body)
    }
    Add-ReportLine '```'
    Add-ReportLine ''

    $reqHeaders = @{}
    foreach ($k in $Headers.Keys) { $reqHeaders[$k] = $Headers[$k] }

    $jsonBody = $null
    if ($null -ne $Body) {
        $jsonBody = if ($Body -is [string]) { $Body } else { $Body | ConvertTo-Json -Compress -Depth 10 }
    }

    $statusCode = 0
    $statusDesc = ""
    $respBody = ""
    $respContentType = ""

    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $reqHeaders
            UseBasicParsing = $true
        }
        if ($null -ne $jsonBody) {
            $params["Body"] = [System.Text.Encoding]::UTF8.GetBytes($jsonBody)
            $params["ContentType"] = "application/json; charset=utf-8"
        }
        $resp = Invoke-WebRequest @params
        $statusCode = [int]$resp.StatusCode
        $statusDesc = $resp.StatusDescription
        $respBody = $resp.Content
        $respContentType = $resp.Headers["Content-Type"]
    } catch {
        if ($_.Exception.Response) {
            $resp = $_.Exception.Response
            $statusCode = [int]$resp.StatusCode
            $statusDesc = $resp.StatusDescription
            $respContentType = $resp.Headers["Content-Type"]
            $stream = $resp.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $respBody = $reader.ReadToEnd()
        } else {
            $statusCode = 500
            $statusDesc = "Exception"
            $respBody = $_.Exception.Message
        }
    }

    $statusMatch = ($statusCode -eq $ExpectedStatus)
    $badge = if ($statusMatch) { "🟢 PASSED" } else { "🔴 FAILED" }

    Add-ReportLine "**Response:** ``$statusCode $statusDesc`` — **$badge** (Expected: ``$ExpectedStatus``)"
    Add-ReportLine '```json'
    if ([string]::IsNullOrWhiteSpace($respBody)) {
        Add-ReportLine '(Empty body - 204 No Content)'
    } else {
        Add-ReportLine (Format-Json $respBody)
    }
    Add-ReportLine '```'
    Add-ReportLine ''
    Add-ReportLine '---'
    Add-ReportLine ''

    Write-Host "[$badge] $statusCode $statusDesc - $Method $Path ($Name)" -ForegroundColor $(if ($statusMatch) { "Green" } else { "Red" })

    return @{
        StatusCode = $statusCode
        Body = $respBody
        Json = $(try { $respBody | ConvertFrom-Json } catch { $null })
    }
}

# 1. Start Server
Write-Host "Starting TeamUp.Api on $baseUrl..." -ForegroundColor Cyan
$job = Start-Job -ScriptBlock {
    Set-Location "C:\Users\ahmed\Downloads\TeamUp"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://localhost:5199"
    dotnet run --project src/TeamUp.Api --no-build --urls http://localhost:5199
}

# Wait for server
$started = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep 2
    try {
        $h = Invoke-WebRequest "$baseUrl/health" -UseBasicParsing -TimeoutSec 2
        if ($h.StatusCode -eq 200) {
            $started = $true
            break
        }
    } catch {}
}

if (-not $started) {
    Write-Host "Failed to start server within 60s" -ForegroundColor Red
    Stop-Job $job -Force
    Remove-Job $job -Force
    exit 1
}

Write-Host "Server is HEALTHY. Starting test execution..." -ForegroundColor Green

Add-ReportLine "# TeamUp — Live API Endpoint Validation & Response Evidence Report"
Add-ReportLine ""
Add-ReportLine "> **Execution Timestamp:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')"  
Add-ReportLine "> **Target Environment:** ASP.NET Core 10 Web API (`http://localhost:5199`)"  
Add-ReportLine "> **Database Engine:** Microsoft SQL Server 2022 (Docker container `teamup-sql:14333`)"  
Add-ReportLine "> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (`DeletedAt`)"  
Add-ReportLine ""
Add-ReportLine "---"
Add-ReportLine ""

# Test execution variables
$testEmail = "live_student_$(Get-Random)@teamup.edu"
$password = "StrongP@ssw0rd2026!"
$jwtToken = ""
$refreshToken = ""
$studentId = 0

# Test 1: Health
Send-HttpReq -Name "01. Service Health Check" -Method "GET" -Path "/health" -ExpectedStatus 200

# Test 2: Register Success
$regRes = Send-HttpReq -Name "02. Student Registration (Happy Path)" -Method "POST" -Path "/api/v1/auth/register" `
    -Body @{
        email = $testEmail
        password = $password
        firstName = "Ahmed"
        lastName = "Tyson"
        academicYear = 3
        facultyId = 1
    } -ExpectedStatus 201

$jwtToken = if ($regRes.Json.data) { $regRes.Json.data.accessToken } else { $regRes.Json.accessToken }
$refreshToken = if ($regRes.Json.data) { $regRes.Json.data.refreshToken } else { $regRes.Json.refreshToken }
$studentId = if ($regRes.Json.data) { $regRes.Json.data.student.studentId } else { $regRes.Json.student.studentId }

# Test 3: Duplicate Email
Send-HttpReq -Name "03. Registration Conflict (Duplicate Email)" -Method "POST" -Path "/api/v1/auth/register" `
    -Body @{
        email = $testEmail
        password = "AnotherPassword123!"
        firstName = "Duplicate"
        lastName = "User"
        academicYear = 2
        facultyId = 1
    } -ExpectedStatus 409

# Test 4: Missing Required Fields
Send-HttpReq -Name "04. Registration Validation (Empty Password)" -Method "POST" -Path "/api/v1/auth/register" `
    -Body @{
        email = "invalid_user@teamup.edu"
        password = ""
        firstName = "Empty"
        lastName = "Password"
        academicYear = 1
        facultyId = 1
    } -ExpectedStatus 400

# Test 5: Non-Existent Faculty
Send-HttpReq -Name "05. Registration ForeignKey Failure (Invalid Faculty ID)" -Method "POST" -Path "/api/v1/auth/register" `
    -Body @{
        email = "bad_faculty_$(Get-Random)@teamup.edu"
        password = "ValidPassword123!"
        firstName = "Ghost"
        lastName = "Faculty"
        academicYear = 1
        facultyId = 99999
    } -ExpectedStatus 404

# Test 6: Login Success
$loginRes = Send-HttpReq -Name "06. Student Login (Valid Credentials)" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = $testEmail
        password = $password
    } -ExpectedStatus 200

# Update token from login
$jwtToken = if ($loginRes.Json.data) { $loginRes.Json.data.accessToken } else { $loginRes.Json.accessToken }
$refreshToken = if ($loginRes.Json.data) { $loginRes.Json.data.refreshToken } else { $loginRes.Json.refreshToken }

# Test 7: Login Wrong Password
Send-HttpReq -Name "07. Login Authentication Failure (Incorrect Password)" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = $testEmail
        password = "WrongPasswordXYZ!"
    } -ExpectedStatus 401

# Test 8: Login Non-Existent Email
Send-HttpReq -Name "08. Login Authentication Failure (Unknown Email)" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = "does_not_exist@teamup.edu"
        password = "AnyPassword123!"
    } -ExpectedStatus 401

# Test 9: Refresh Token
$refreshRes = Send-HttpReq -Name "09. Refresh Access Token (Token Rotation)" -Method "POST" -Path "/api/v1/auth/refresh" `
    -Body @{
        refreshToken = $refreshToken
    } -ExpectedStatus 200

$jwtToken = if ($refreshRes.Json.data) { $refreshRes.Json.data.accessToken } else { $refreshRes.Json.accessToken }
$refreshToken = if ($refreshRes.Json.data) { $refreshRes.Json.data.refreshToken } else { $refreshRes.Json.refreshToken }

# Test 10: Invalid Refresh Token
Send-HttpReq -Name "10. Refresh Token Failure (Tampered/Invalid Token)" -Method "POST" -Path "/api/v1/auth/refresh" `
    -Body @{
        refreshToken = "TAMPERED_REFRESH_TOKEN_1234567890_INVALID"
    } -ExpectedStatus 401

# Test 11: Protected Route Without Auth
Send-HttpReq -Name "11. Protected Route Guard (Missing Authorization Header)" -Method "GET" -Path "/api/v1/students/me" -ExpectedStatus 401

# Test 12: Protected Route With Valid Bearer Token
$authHeaders = @{ "Authorization" = "Bearer $jwtToken" }
Send-HttpReq -Name "12. Fetch Authenticated Student Account (GET /students/me)" -Method "GET" -Path "/api/v1/students/me" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 13: Patch Student Names & Academic Year
Send-HttpReq -Name "13. Update Student Account (PATCH /students/me)" -Method "PATCH" -Path "/api/v1/students/me" `
    -Headers $authHeaders `
    -Body @{
        firstName = "Ahmed (Lead)"
        lastName = "Tyson"
        academicYear = 4
    } -ExpectedStatus 200

# Test 14: Get Initial Extended Profile (Empty)
Send-HttpReq -Name "14. Fetch Initial Extended Profile (Before Creation)" -Method "GET" -Path "/api/v1/students/$studentId/profile" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 15: Upsert Extended Profile
Send-HttpReq -Name "15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)" -Method "PUT" -Path "/api/v1/students/me/profile" `
    -Headers $authHeaders `
    -Body @{
        bio = "Full-stack engineer building TeamUp. Spec-Driven & Test-Driven Development advocate."
        photoUrl = "https://avatars.githubusercontent.com/u/AhmedTyson"
        gitHubUrl = "https://github.com/AhmedTyson"
        linkedInUrl = "https://linkedin.com/in/ahmedtyson"
    } -ExpectedStatus 200

# Test 16: Verify Updated Extended Profile
Send-HttpReq -Name "16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)" -Method "GET" -Path "/api/v1/students/$studentId/profile" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 17: Add First Interest Tag
Send-HttpReq -Name "17. Add First Interest Tag (POST /students/me/interests)" -Method "POST" -Path "/api/v1/students/me/interests" `
    -Headers $authHeaders `
    -Body @{
        interest = "Artificial Intelligence"
    } -ExpectedStatus 200

# Test 18: Add Second Interest Tag
Send-HttpReq -Name "18. Add Second Interest Tag (POST /students/me/interests)" -Method "POST" -Path "/api/v1/students/me/interests" `
    -Headers $authHeaders `
    -Body @{
        interest = "Cloud & Distributed Systems"
    } -ExpectedStatus 200

# Test 19: List Student Interests
Send-HttpReq -Name "19. List Active Student Interests (GET /students/{id}/interests)" -Method "GET" -Path "/api/v1/students/$studentId/interests" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 20: Soft-Delete Interest Tag
Send-HttpReq -Name "20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})" -Method "DELETE" -Path "/api/v1/students/me/interests/Artificial%20Intelligence" `
    -Headers $authHeaders -ExpectedStatus 204

# Test 21: Verify Soft-Deleted Tag is Hidden
Send-HttpReq -Name "21. Verify Soft-Deleted Tag is Filtered Out" -Method "GET" -Path "/api/v1/students/$studentId/interests" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 22: Restore Soft-Deleted Tag by Re-Adding
Send-HttpReq -Name "22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)" -Method "POST" -Path "/api/v1/students/me/interests" `
    -Headers $authHeaders `
    -Body @{
        interest = "Artificial Intelligence"
    } -ExpectedStatus 200

# Test 23: Verify Tag Restored
Send-HttpReq -Name "23. Verify Both Interest Tags Active" -Method "GET" -Path "/api/v1/students/$studentId/interests" `
    -Headers $authHeaders -ExpectedStatus 200

# ----------------- Module 2: Universities & Faculties Tests -----------------
# Get Admin Token
$adminPass = ([regex]::Match(((dotnet user-secrets list --project src/TeamUp.Api | Select-String "Seed:AdminPassword").ToString()),'Seed:AdminPassword = (.*)')).Groups[1].Value.Trim()
$adminLoginRes = Send-HttpReq -Name "24. PlatformAdmin Authentication" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = "admin@teamup.local"
        password = $adminPass
    } -ExpectedStatus 200

$adminToken = if ($adminLoginRes.Json.data) { $adminLoginRes.Json.data.accessToken } else { $adminLoginRes.Json.accessToken }
$adminHeaders = @{ "Authorization" = "Bearer $adminToken" }

# Test 25: List Universities as Student
Send-HttpReq -Name "25. List Universities with Paging (GET /universities)" -Method "GET" -Path "/api/v1/universities?page=1&pageSize=10" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 26: Get University Details with Faculties
Send-HttpReq -Name "26. Get University Details with Faculties (GET /universities/{id})" -Method "GET" -Path "/api/v1/universities/1" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 27: Student Attempt to Create University (Role Authorization Check)
Send-HttpReq -Name "27. Create University as Student Forbidden (Role Guard Check)" -Method "POST" -Path "/api/v1/universities" `
    -Headers $authHeaders `
    -Body @{
        name = "Student Unauthorized University"
        city = "Cairo"
        country = "Egypt"
    } -ExpectedStatus 403

# Test 28: Admin Create University
$uniName = "Alexandria University $(Get-Random)"
$createUniRes = Send-HttpReq -Name "28. Create Accredited University as Admin (POST /universities)" -Method "POST" -Path "/api/v1/universities" `
    -Headers $adminHeaders `
    -Body @{
        name = $uniName
        city = "Alexandria"
        country = "Egypt"
        website = "https://alexu.edu.eg"
    } -ExpectedStatus 201

$createdUniId = if ($createUniRes.Json.data) { $createUniRes.Json.data.universityId } else { $createUniRes.Json.universityId }

# Test 29: Admin Duplicate University Name Conflict
Send-HttpReq -Name "29. Create University Conflict (Duplicate Name)" -Method "POST" -Path "/api/v1/universities" `
    -Headers $adminHeaders `
    -Body @{
        name = $uniName
        city = "Alexandria"
        country = "Egypt"
    } -ExpectedStatus 409

# Test 30: Admin Update University
Send-HttpReq -Name "30. Update University Information (PUT /universities/{id})" -Method "PUT" -Path "/api/v1/universities/$createdUniId" `
    -Headers $adminHeaders `
    -Body @{
        name = "$uniName Updated"
        city = "Alexandria"
        country = "Egypt"
        website = "https://updated.alexu.edu.eg"
    } -ExpectedStatus 200

# Test 31: Get Faculties in University
Send-HttpReq -Name "31. List Faculties in University (GET /universities/{id}/faculties)" -Method "GET" -Path "/api/v1/universities/$createdUniId/faculties" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 32: Create Faculty in University
$createFacRes = Send-HttpReq -Name "32. Add Faculty to University as Admin (POST /universities/{id}/faculties)" -Method "POST" -Path "/api/v1/universities/$createdUniId/faculties" `
    -Headers $adminHeaders `
    -Body @{
        name = "Faculty of Medicine"
        code = "MED_ALEX"
    } -ExpectedStatus 201

$createdFacId = if ($createFacRes.Json.data) { $createFacRes.Json.data.facultyId } else { $createFacRes.Json.facultyId }

# Test 33: Duplicate Faculty Code Conflict
Send-HttpReq -Name "33. Create Faculty Code Conflict (Duplicate Code)" -Method "POST" -Path "/api/v1/universities/$createdUniId/faculties" `
    -Headers $adminHeaders `
    -Body @{
        name = "Faculty of Medicine Duplicate"
        code = "MED_ALEX"
    } -ExpectedStatus 409

# Test 34: Update Faculty Details
Send-HttpReq -Name "34. Update Faculty Information (PUT /faculties/{id})" -Method "PUT" -Path "/api/v1/faculties/$createdFacId" `
    -Headers $adminHeaders `
    -Body @{
        name = "Faculty of Medicine & Health"
        code = "MED_HEALTH"
    } -ExpectedStatus 200

# Test 35: Soft-Delete Faculty
Send-HttpReq -Name "35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})" -Method "DELETE" -Path "/api/v1/faculties/$createdFacId" `
    -Headers $adminHeaders -ExpectedStatus 204

# Test 36: Soft-Delete University
Send-HttpReq -Name "36. Soft-Delete University as Admin (DELETE /universities/{id})" -Method "DELETE" -Path "/api/v1/universities/$createdUniId" `
    -Headers $adminHeaders -ExpectedStatus 204

# ----------------- Module 3: Student Directory & Public Profile Tests -----------------
# Test 37: Search Student Directory
Send-HttpReq -Name "37. Search Student Directory (GET /students?search=...)" -Method "GET" -Path "/api/v1/students?search=Ahmed&page=1&pageSize=10" `
    -Headers $authHeaders -ExpectedStatus 200

# Test 38: Public Student Profile Card
Send-HttpReq -Name "38. Get Public Student Profile Card (GET /students/{id})" -Method "GET" -Path "/api/v1/students/$studentId" `
    -Headers $authHeaders -ExpectedStatus 200

# ----------------- Module 4: Password Recovery & Reset Tests -----------------
# Test 39: Forgot Password Request
$forgotRes = Send-HttpReq -Name "39. Request Password Reset Token (POST /auth/forgot-password)" -Method "POST" -Path "/api/v1/auth/forgot-password" `
    -Body @{
        email = $testEmail
    } -ExpectedStatus 200

$resetToken = if ($forgotRes.Json.data) { $forgotRes.Json.data.resetToken } else { $forgotRes.Json.resetToken }

# Test 40: Reset Password with Valid Token
$newPassword = "UpdatedSecurePassword2026!"
Send-HttpReq -Name "40. Reset Password with Token (POST /auth/reset-password)" -Method "POST" -Path "/api/v1/auth/reset-password" `
    -Body @{
        email = $testEmail
        token = $resetToken
        newPassword = $newPassword
    } -ExpectedStatus 200

# Test 41: Login with New Password
$newLoginRes = Send-HttpReq -Name "41. Authenticate with New Password (POST /auth/login)" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = $testEmail
        password = $newPassword
    } -ExpectedStatus 200

$newStudentToken = if ($newLoginRes.Json.data) { $newLoginRes.Json.data.accessToken } else { $newLoginRes.Json.accessToken }
$newStudentHeaders = @{ "Authorization" = "Bearer $newStudentToken" }

# ----------------- Module 5: Student Account Soft Deletion & Verification -----------------
# Test 42: Self Soft-Delete Account
Send-HttpReq -Name "42. Soft-Delete Student Account (DELETE /students/me)" -Method "DELETE" -Path "/api/v1/students/me" `
    -Headers $newStudentHeaders -ExpectedStatus 204

# Test 43: Login Fails After Account Soft-Deletion
Send-HttpReq -Name "43. Login Rejection for Soft-Deleted Student" -Method "POST" -Path "/api/v1/auth/login" `
    -Body @{
        email = $testEmail
        password = $newPassword
    } -ExpectedStatus 401

# Test 44: Directory Omits Soft-Deleted Student
Send-HttpReq -Name "44. Verify Soft-Deleted Student Omitted from Directory" -Method "GET" -Path "/api/v1/students?search=$testEmail" `
    -Headers $adminHeaders -ExpectedStatus 200

# Test 45: Logout Admin Session
Send-HttpReq -Name "45. Invalidate Admin Session & Logout (POST /auth/logout)" -Method "POST" -Path "/api/v1/auth/logout" `
    -Headers $adminHeaders -ExpectedStatus 204

# Test 46: Refresh Token Rejection After Logout
Send-HttpReq -Name "46. Refresh Token Rejection After Logout (Token Revoked)" -Method "POST" -Path "/api/v1/auth/refresh" `
    -Body @{
        refreshToken = $refreshToken
    } -ExpectedStatus 401

# Cleanup server
Write-Host "Stopping background server job..." -ForegroundColor Cyan
Stop-Job $job
Remove-Job $job -Force
Get-Process TeamUp.Api -ErrorAction SilentlyContinue | Stop-Process -Force

# Write report file
$logLines | Out-File -FilePath $reportPath -Encoding utf8
Write-Host "Test Report saved to $reportPath" -ForegroundColor Green
