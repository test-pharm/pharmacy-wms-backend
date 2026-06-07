import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
AUDIT_LOG_URL = f"{BASE_URL}/api/AuditLog"  # Note the capital 'A' and 'L' as per instructions

def test_get_audit_log_with_admin_authorization():
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        # Authenticate as admin to get JWT token
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=30)
        assert login_response.status_code == 200, f"Login failed with status code {login_response.status_code}"
        token = login_response.json().get("token")
        assert token is not None and isinstance(token, str) and len(token) > 0, "Token not received or invalid"
        
        headers = {
            "Authorization": f"Bearer {token}"
        }
        
        # Get audit logs with admin authorization
        response = requests.get(AUDIT_LOG_URL, headers=headers, timeout=30)
        assert response.status_code == 200, f"Audit log fetch failed with status code {response.status_code}"
        
        json_data = response.json()
        # Check that 'data' key exists and is a list (paginated audit log results)
        assert "data" in json_data, "'data' key missing in audit log response"
        assert isinstance(json_data["data"], list), "'data' is not a list in audit log response"
        
    except requests.RequestException as e:
        assert False, f"Request failed: {e}"

test_get_audit_log_with_admin_authorization()