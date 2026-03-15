import http from "k6/http";
import { check } from "k6";
import { API_BASE_URL, TENANT_ID } from "../config/config.js";
import { getToken } from "../utils/auth.js"

export function getTasks(username, password) {
    const token = getToken(username, password);

    const params = {
        headers: {
            "Authorization": `Bearer ${token}`,
            "X-Tenant-Id": TENANT_ID
        }
    };

    const res = http.get(`${API_BASE_URL}/api/Tasks/`, params);

    check(res, {
        "status is 200": (r) => r.status === 200,
    });
}

export function getTaskById(taskId, username, password) {
    const token = getToken(username, password);

    const params = {
        headers: {
            "Authorization": `Bearer ${token}`,
            "X-Tenant-Id": TENANT_ID
        }
    };

    const res = http.get(`${API_BASE_URL}/api/Tasks/${taskId}`, params);

    check(res, {
        "status is 200": (r) => r.status === 200,
        "has body": (r) => r.body && r.body.length > 0
    });
}

