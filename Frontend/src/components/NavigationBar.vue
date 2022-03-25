<template>
  <div>
    <Toast position="top-right" />
    <div
      v-if="isLogged"
      id="nav"
    >
      <TabMenu :model="items" />
    </div>
  </div>
</template>

<script>
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import Toast from "primevue/toast";
import TabMenu from "primevue/tabmenu";

export default {
  name: "NavigationBar",
  components: { Toast, TabMenu },
  data() {
    return {
      items: [
        {label: "Settings", icon: "pi pi-fw pi-cog", to: "/settings", text: "Settings"},
        {label: "My exchanges", icon: "pi pi-fw pi-inbox", to: "/exchanges", text: "My exchanges"},
        {label: "My timetable", icon: "pi pi-fw pi-calendar", to: "/", text: "My timetable"},
        {label: "My teams", icon: "pi pi-fw pi-users", to: "/groups", text: "My teams"},
        {label: "Messages", icon: "pi pi-fw pi-inbox", to: "/messages", text: "Messages"}
      ]
    };
  },
  computed: {
    ...mapGetters([
      "isLogged",
      "getErrorMsg",
      "getSuccessMsg",
      "getWarnMsg",
      "getInfoMsg",
      "getString",
      "getLanguage"
    ]),
    currentRouteName() {
      return this.$route.name;
    }
  },
  watch: {
    getErrorMsg: function () {
      if(this.getErrorMsg.length > 0) {
        this.$toast.add({severity:"error", summary: "", detail: this.getString(this.getErrorMsg), life: 5000, closable: false});
        this.clearError();
      }
    },
    getWarnMsg: function () {
      if(this.getWarnMsg.length > 0) {
        this.$toast.add({severity:"warn", summary: "", detail: this.getString(this.getWarnMsg), life: 5000, closable: false});
        this.clearWarn();
      }
    },
    getInfoMsg: function () {
      if(this.getInfoMsg.length > 0) {
        this.$toast.add({severity:"info", summary: "", detail: this.getString(this.getInfoMsg), life: 5000, closable: false});
        this.clearInfo();
      }
    },
    getSuccessMsg: function () {
      if(this.getSuccessMsg.length > 0) {
        this.$toast.add({severity:"success", summary: "", detail: this.getString(this.getSuccessMsg), life: 5000, closable: false});
        this.clearSuccess();
      }
    },
    getLanguage: function() {
      if(this.getString(this.items[0].text) !== this.items[0].label) {
        this.updateBar();
      }
    }
  },
  created() {
    this.updateBar();
    window.addEventListener("resize", this.updateBar);
  },
  methods: {
    ...mapActions(["clearError", "clearSuccess", "clearWarn", "clearInfo"]),
    isMobile() { return screen.width <= 820; },
    updateBar() { this.items.forEach(x => {
      if(this.isMobile()) {
        x.label = "";
      } else {
        x.label = this.getString(x.text);
      }
    });}
  }
};
</script>

<style>
li a {
  text-decoration: none;
  color: aliceblue;
}

.p-tabmenu,
.p-tabmenu-nav {
  border-width: 0 0 3px 0 !important;
  width: 100%;
}

.p-tabmenuitem {
  width: 20%;
}

a.p-menuitem-link {
  text-align: center;
  width: 100%;
  display: block;
}
</style>
