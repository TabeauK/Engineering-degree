<template>
  <div class="home">
    <Auth
      :msg="msg ? msg : ''"
      :type="type ? type : ''"
    />
  </div>
</template>

<script>
import Auth from "../components/Auth";

export default {
  name: "Login",
  components: {
    Auth
  },
  props: {
    msg: {
      type: String,
      default: ""
    },
    type: {
      type: String,
      default: ""
    }
  }
};
</script>
