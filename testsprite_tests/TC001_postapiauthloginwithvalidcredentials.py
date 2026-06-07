import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
TIMEOUT = 30

def test_postapiauthloginwithvalidcredentials():
    headers = {"Content-Type": "application/json"}
    valid_credentials = [
        {"email": "admin@pharmacy.com", "password": "admin123"},
        {"email": "supervisor@pharmacy.com", "password": "super123"},
    ]
    for creds in valid_credentials:
        try:
            response = requests.post(LOGIN_URL, json=creds, headers=headers, timeout=TIMEOUT)
        except requests.RequestException as e:
            assert False, f"Request failed: {e}"
        assert response.status_code == 200, f"Expected status code 200, got {response.status_code}"
        json_data = response.json()
        assert "token" in json_data and isinstance(json_data["token"], str) and len(json_data["token"]) > 0, "JWT token missing or invalid in response"

test_postapiauthloginwithvalidcredentials()