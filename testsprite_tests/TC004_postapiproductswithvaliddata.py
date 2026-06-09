import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
PRODUCTS_URL = f"{BASE_URL}/api/products"
TIMEOUT = 30

def test_post_api_products_with_valid_data():
    # Step 1: Authenticate as admin to get token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    login_resp = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
    assert login_resp.status_code == 200, f"Login failed with status {login_resp.status_code}"
    login_data = login_resp.json()
    token = login_data.get("token")
    assert token, "JWT token not found in login response"

    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    # Prepare valid product payload according to instruction #3 of TC004 (no name, price, description)
    product_payload = {
        "materialName": "TestMaterial",
        "materialSku": "TM-001",
        "quantity": 100,
        "unit": "kg",
        "logNumber": "LOG123456",
        "supplier": "TestSupplier",
        "categoryId": 1
    }

    resp = None
    created_product_id = None

    try:
        resp = requests.post(PRODUCTS_URL, json=product_payload, headers=headers, timeout=TIMEOUT)
        assert resp.status_code == 201, f"Expected status 201 Created but got {resp.status_code}"
        resp_data = resp.json()
        created_product_id = resp_data.get("id")
        assert created_product_id is not None, "Created product id not returned"
        # Optionally verify returned fields contain the sent data
        for key in product_payload:
            assert resp_data.get(key) == product_payload[key], f"Mismatch in field {key}"
    finally:
        # Clean up: delete created product if exists
        if created_product_id:
            delete_url = f"{PRODUCTS_URL}/{created_product_id}"
            del_resp = requests.delete(delete_url, headers=headers, timeout=TIMEOUT)
            # Per PRD, DELETE /api/products returns 204 No Content on success
            assert del_resp.status_code == 204, f"Failed to delete product with id {created_product_id}"

test_post_api_products_with_valid_data()