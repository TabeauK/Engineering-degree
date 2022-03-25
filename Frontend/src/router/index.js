import { createRouter, createWebHashHistory } from "vue-router";
import Login from "../views/Login.vue";
import Verify from "../views/Verify.vue";
import Timetable from "../views/Timetable.vue";
import Course from "../views/Course.vue";
import CourseGroup from "../views/CourseGroup.vue";
import Groups from "../views/Groups.vue";
import Messages from "../views/Messages.vue";
import Exchanges from "../views/Exchanges.vue";
import Settings from "../views/Settings.vue";

const routes = [
  {
    path: "/",
    name: "Timetable",
    component: Timetable,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/course/:id",
    name: "Course",
    component: Course,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/course_group/:id",
    name: "CourseGroup",
    component: CourseGroup,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/groups",
    name: "Groups",
    component: Groups,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/messages",
    name: "Messages",
    component: Messages,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/exchanges",
    name: "Exchanges",
    component: Exchanges,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/settings",
    name: "Settings",
    component: Settings,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/login",
    name: "Login",
    component: Login,
    props: true,
    meta: {
      requiresAuth: false
    }
  },
  {
    path: "/verify",
    name: "Verify",
    component: Verify,
    meta: {
      requiresAuth: false
    }
  }
];

const router = createRouter({
  history: createWebHashHistory(),
  routes
});

export default router;
