import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
ORDERS_URL = f"{BASE_URL}/api/orders"
TIMEOUT = 30

def test_get_api_orders_list_with_valid_token():
    # Step 1: Login to get valid JWT token (use admin credentials as example)
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed with status code {login_response.status_code}"
        token = login_response.json().get("token")
        assert token and isinstance(token, str), "JWT token not found in login response"
    except requests.RequestException as e:
        assert False, f"Login request failed: {e}"

    # Step 2: Make GET request to /api/orders with the valid token
    headers = {
        "Authorization": f"Bearer {token}"
    }
    try:
        orders_response = requests.get(ORDERS_URL, headers=headers, timeout=TIMEOUT)
    except requests.RequestException as e:
        assert False, f"GET /api/orders request failed: {e}"

    # Step 3: Assert the response status code is 200
    assert orders_response.status_code == 200, f"Expected status code 200 but got {orders_response.status_code}"

    # Step 4: Assert the response contains a list (assumption: list can be empty)
    try:
        orders_data = orders_response.json()
    except ValueError:
        assert False, "Response is not valid JSON"

    # Since PRD mentions "a list of warehouse stock movements", a likely structure is a list at root or maybe under a key
    # We check if it's a list or contains a 'data' or similar key with a list - we assume it's a list at root from PRD usage
    assert isinstance(orders_data, list), f"Expected response to be a list but got {type(orders_data)}"
    
    # Optional: If list is not empty, check if each item has expected basic keys (like id, date, type)
    # This detailed check is not explicitly required so we skip it

test_get_api_orders_list_with_valid_token()