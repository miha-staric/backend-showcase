
import { getTasks, getTaskById } from "../endpoints/tasks.js";
import { TEST_USERS, EXISTING_TASKS } from "../config/config.js";

export const options = {
    stages: [
        { duration: "10s", target: 5 },   // ramp up
        { duration: "30s", target: 20 },  // sustain
        { duration: "10s", target: 0 }    // ramp down
    ]
};

export default function () {
    const user = TEST_USERS[Math.floor(Math.random() * TEST_USERS.length)];
    const existingTask = EXISTING_TASKS[Math.floor(Math.random() * EXISTING_TASKS.length)];

    getTasks(user.username, user.password);
    getTaskById(existingTask, user.username, user.password);
}

