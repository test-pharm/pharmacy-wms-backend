import requests

BASE_URL = "http://localhost:10000"

def test_get_api_categories_with_valid_token():
    login_url = f"{BASE_URL}/api/auth/login"
    categories_url = f"{BASE_URL}/api/categories"
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        # Authenticate to get JWT token
        login_resp = requests.post(login_url, json=login_payload, timeout=30)
        assert login_resp.status_code == 200, f"Login failed with status code {login_resp.status_code}"
        login_json = login_resp.json()
        assert "token" in login_json, "JWT token not found in login response"
        token = login_json["token"]

        headers = {
            "Authorization": f"Bearer {token}"
        }

        # GET /api/categories with valid token
        resp = requests.get(categories_url, headers=headers, timeout=30)
        assert resp.status_code == 200, f"Expected status 200, got {resp.status_code}"
        json_data = resp.json()
        assert isinstance(json_data, list) or (isinstance(json_data, dict) and "data" in json_data), \
            "Response should be a list or contain 'data' key"

    except requests.RequestException as e:
        assert False, f"Request failed: {e}"

test_get_api_categories_with_valid_token()