<template>
  <div class="button-wrapper">
    <h1 class="title">
      {{ getString("Welcome to UsosFix") }}
    </h1>
    <div class="login">
      <Button
        class="button"
        :label="getString('Login')"
        @click="tryLogin"
      />
      <div>
        <Message
          v-if="type == 'logout'"
          severity="success"
          :life="3000"
          :sticky="false"
        >
          {{ getString(msg) }}
        </Message>
        <Message
          v-else-if="type == 'expired'"
          severity="error"
          :life="3000"
          :sticky="false"
        >
          {{ getString(msg) }}
        </Message>
        <Message
          v-else-if="type == 'error'"
          severity="error"
          :life="3000"
          :sticky="false"
        >
          {{ getString(msg) }}
        </Message>
      </div>
    </div>
  </div>
</template>

<script>
import Message from "primevue/message";
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import Button from "primevue/button";

export default {
  name: "Auth",
  components: { Message, Button },
  props: {
    msg: {
      type: String,
      default: ""
    },
    type: {
      type: String,
      default: ""
    }
  },
  computed: mapGetters(["getString", "getLang"]),
  methods: {
    ...mapActions(["login"]),
    tryLogin() {
      this.login();
    }
  }
};
</script>

<style scoped>
.button {
  font-size: 24px;
  max-width: 600px;
  width: 80%;
  margin: 0 0 30px;
  align-items: center;
  box-shadow: 0 35px 50px rgba(0, 0, 0, 0.2);
  border-radius: 40px;
  min-height: 75px;
  padding: 0 30px;
  transition: background 0.3s, transform 0.3s;
  outline: 0;
  border: none;
}

.button-wrapper {
  height: 100vh;
  background: url(~@/assets/BG_UsosFix.svg) no-repeat fixed center;
  background-size: cover;
  padding: 50px;
  justify-content: center;
  align-content: center;
  align-items: center;
}

.title {
  position: relative;
  top: calc(25% - 75px);
  font-size: 6vh;
}

.login {
  position: relative;
  left: 50%;
  min-height: 20%;
  transform: translateX(-50%);
  top: calc(50%);
  max-width: 600px;
}

.login-screen {
  background-color: #fff;
  padding: 20px;
  border-radius: 5px;
}

.app-title {
  text-align: center;
  color: #777;
  cursor: default;
}

a {
  cursor: pointer;
  text-align: center;
}

.btn {
  border: 2px solid transparent;
  background: #3498db;
  color: #fff;
  font-size: 16px;
  line-height: 25px;
  text-decoration: none;
  text-shadow: none;
  border-radius: 3px;
  box-shadow: none;
  transition: 0.5s;
  display: block;
  width: 100%;
  margin: 0;
}

.btn:hover {
  box-shadow: inset 300px 0 0 0 blue;
  background-color: #2980b9;
}

.msg {
  position: absolute;
  top: 0%;
  color: red;
}
</style>
