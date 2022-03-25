import { createApp } from "vue";
import App from "./App.vue";
import ConfirmationService from "primevue/confirmationservice";
import PrimeVue from "primevue/config";
import router from "./router";
import store from "./store";
import ToastService from "primevue/toastservice";
import axios from "axios";
import "primevue/resources/primevue.min.css";
import "primeicons/primeicons.css";
import "primevue/resources/themes/saga-blue/theme.css";
import "vue-cal/dist/vuecal.css";
import "primeflex/primeflex.css";
import ApiPath from "./config";

if (!String.format) {
  String.format = function(format) {
    var args = Array.prototype.slice.call(arguments, 1);
    return format.replace(/{(\d+)}/g, function(match, number) { 
      return typeof args[number] != "undefined" ? 
        args[number] 
        : match
      ;
    });
  };
}

axios.interceptors.response.use(function (response) {
  if(!response.expired) {
    return response.data;
  }
  store.commit("logOut");
  router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
  return Promise.reject(response);
}, function (error) {
  if (error.response.status === 401) {
    store.commit("logOut");
    router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    return Promise.reject(true);
  } else {
    return Promise.reject(false);
  }
});

axios.defaults.baseURL = ApiPath();

router.beforeEach((to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!store.getters.isLogged) {
      next({ name: "Verify" });
    } else {
      next();
    }
  } else {
    if (store.getters.isLogged) {
      next({ name: "Timetable" });
    } else {
      next();
    }
  }
});

createApp(App).use(store).use(router).use(ToastService).use(ConfirmationService).use(PrimeVue).mount("#app");