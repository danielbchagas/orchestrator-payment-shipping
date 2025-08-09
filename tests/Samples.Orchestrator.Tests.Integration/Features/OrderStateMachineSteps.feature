Feature: Order State Machine Integration

  Scenario: Finalizar pedido com sucesso
    Given que inicio uma nova saga de pedido
    When envio um Payment.Submitted válido
    And envio um Payment.Accepted
    And envio um Shipping.Submitted válido
    And envio um Shipping.Accepted
    Then o estado final deve ser FinalState
    And um evento FinalEvent deve ser publicado