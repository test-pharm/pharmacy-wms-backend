import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
AUDIT_LOG_URL = f"{BASE_URL}/api/AuditLog"
TIMEOUT = 30

def test_getapiauditlogwithadminauthorization():
    # Step 1: Login as admin to get JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
    except requests.RequestException as e:
        assert False, f"Admin login request failed: {e}"
    assert login_response.status_code == 200, f"Expected 200 OK from login, got {login_response.status_code}"
    login_json = login_response.json()
    assert "token" in login_json, "JWT token not found in login response"
    token = login_json["token"]

    # Step 2: Get audit log with admin JWT token
    headers = {"Authorization": f"Bearer {token}"}
    try:
        audit_log_response = requests.get(AUDIT_LOG_URL, headers=headers, timeout=TIMEOUT)
    except requests.RequestException as e:
        assert False, f"Audit log request failed: {e}"

    assert audit_log_response.status_code == 200, f"Expected 200 OK from audit log, got {audit_log_response.status_code}"
    audit_log_json = audit_log_response.json()
    assert "data" in audit_log_json, "Response JSON does not contain 'data' key for audit logs"
    assert isinstance(audit_log_json["data"], list), "'data' is not a list in audit log response"

test_getapiauditlogwithadminauthorization()